using LanguageExt;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Caching;
using WEB.UTILITY.LanguageExt;
using WEB.UTILITY.Logger;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class ReverseProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICache _cache;
    private readonly IOutputCacheService _policyService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IProxyConfigService _routingService;
    private readonly IAppLogger<ReverseProxyMiddleware> _logger;
    private readonly IRateLimitingStrategy _rateLimitStrategy;
    private readonly IErrorHandlingStrategy _errorHandlingStrat;

    public ReverseProxyMiddleware(
        RequestDelegate next,
        ICache cache,
        IOutputCacheService policyService,
        IRateLimitConfigServiceProvider rateLimitService,
        IProxyConfigService routingService,
        IRateLimitingStrategy rateLimitStrategy,
        IErrorHandlingStrategy errorHandlingStrategy,
        IAppLogger<ReverseProxyMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _policyService = policyService;
        _rateLimitService = rateLimitService;
        _routingService = routingService;
        _rateLimitStrategy = rateLimitStrategy;
        _errorHandlingStrat = errorHandlingStrategy;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var routes = await _routingService.GetRoutesAsync();

            if (routes == null)
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Timeout";
                await context.Response.WriteAsync("Gateway Timeout for this request.", context.RequestAborted);
                return;
            }

            string path = context.Request.Path.Value ?? string.Empty;
            static string NormalizePath(string p) => p?.Trim()?.ToLowerInvariant()!;

            var routeMatch = routes
                .Where(r => r.Value?.Match?.Path != null)
                .FirstOrDefault(r =>
                    NormalizePath(r.Value.Match.Path!).Equals(NormalizePath(path), StringComparison.OrdinalIgnoreCase)
                ).Value;


            // endpoint matching
            if (routeMatch == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Route_Path_Not_Found";
                await context.Response.WriteAsync($"Route is not found for this request. Route: '{path}'", context.RequestAborted);
                return;
            }

            //cache policy matching
            string cachePolicyName = routeMatch.OutputCachePolicy ?? string.Empty;
            OutputCachePolicy? policy = _policyService.GetPolicy(cachePolicyName);
            if (policy == null)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Cache_Invalid";
                await context.Response.WriteAsync("Cache Policy is invalid for this request.", context.RequestAborted);
                return;
            }

            string ratePolicyName = routeMatch.RateLimiterPolicy ?? string.Empty;

            //rate limit policy matching
            if (string.IsNullOrEmpty(ratePolicyName) || string.IsNullOrEmpty(routeMatch?.RateLimiterPolicy))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Missing";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync("Rate Limit Policy is missing for this request.", context.RequestAborted);
                return;
            }

            if (!routeMatch.RateLimiterPolicy.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Invalid";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync($"Rate Limit Policy is invalid for this request. Policy: '{ratePolicyName}'", context.RequestAborted);
                return;
            }
            string requestBodyData = await WEB.UTILITY.Helper.StreamReaderHelper.ReadRequestBodyAsync(
        context);

            string clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!(await _rateLimitService.IsRequestAllowed(ratePolicyName, clientId, requestBodyData)))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Exceed";
                await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
                return;
            }

            var proxyFeature = context.Features.Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();

            if (proxyFeature == null)
            {
                _logger.LogDebug("ReverseProxyFeature is missing. Skipping Request.");
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Service_Unavailable";
                await context.Response.WriteAsync("Gateway is unavailable for this request.", context.RequestAborted);
                return;
            }

            var destination = proxyFeature.AvailableDestinations.FirstOrDefault();

            if (destination == null)
            {
                _logger.LogWarning($"No available destination found for Route {routeMatch}.");
                await _errorHandlingStrat.HandleMissingDestinationAsync(context);
                return;
            }

            var allowed = proxyFeature.Route.Config.RateLimiterPolicy?.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase) ?? false;

            if (!allowed)
            {
                _logger.LogDebug($"Rate limit policy: {proxyFeature.Route.Config.RateLimiterPolicy ?? "unknown"} is not allowed for {proxyFeature.Route.Config.Match.Path}.");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Exceed";
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
                return;
            }

            string cacheKey = string.Empty;

            if (isRequestCacheable(context))
            {
                cacheKey = await GenerateCacheKey(context, clientId, requestBodyData);
                var cache = await _cache.Get<string>(cacheKey);

                if (cache.IsSome)
                {
                    _logger.LogDebug($"Fetch from gateway cache. client: {clientId}");
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = StatusCodes.Status200OK;

                    using var data = JsonDocument.Parse(cache.Value());
                    await context.Response.WriteAsync(JsonSerializer.Serialize(data.RootElement,
                        options: new JsonSerializerOptions
                        {
                            WriteIndented = true
                        })
                    , context.RequestAborted);
                    return;
                }
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context); // Proceed to next middleware

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();


            await _cache.Set(cacheKey, responseBody, policy.Duration);

            // Reset stream and copy to original response
            memoryStream.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBodyStream;
            await memoryStream.CopyToAsync(originalBodyStream);
            memoryStream.Close();
            memoryStream.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ReverseProxyServiceMiddleware.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error.");
        }

    }

    private bool isRequestCacheable(HttpContext context)
    {
        return new string[] { "GET", "POST", "PUT" }.Contains(context.Request.Method);
    }

    private async Task<string> GenerateCacheKey(HttpContext context, string clientId, string body)
    {
        var keyBuilder = new StringBuilder($"{context.Request.Path.GetHashCode()}:{clientId.GetHashCode()}:{context.Request.Headers.UserAgent.GetHashCode()}:{context.Request.Headers.Authorization.GetHashCode()}:{body.GetHashCode()}");
        var key = keyBuilder.ToString();
        return key;
    }
}
