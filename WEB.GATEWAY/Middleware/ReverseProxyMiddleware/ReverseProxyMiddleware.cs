using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Logger;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class ReverseProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly IOutputCacheService _policyService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IProxyConfigService _routingService;
    private readonly IAppLogger<ReverseProxyMiddleware> _logger;
    private readonly IRateLimitingStrategy _rateLimitStrategy;
    private readonly IErrorHandlingStrategy _errorHandlingStrat;

    public ReverseProxyMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
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
            var cachePolicyName = context.Request.Headers["X-Cache-Policy"].FirstOrDefault() ?? string.Empty;

            //cache policy matching
            var policy = _policyService.GetPolicy(cachePolicyName);
            if (policy == null)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Cache_Invalid";
                await context.Response.WriteAsync("Cache Policy is invalid for this request.", context.RequestAborted);
                return;
            }

            var path = context.Request.Path;
            var ratePolicyName = context.Request.Headers["X-RateLimit-Policy"].FirstOrDefault() ?? string.Empty;
            var routes = await _routingService.GetRoutesAsync();
            var routeMatch = routes.FirstOrDefault(r => r.Value.Match != null && r.Value.Match.Path != null && r.Value.Match.Path.Equals(path, StringComparison.InvariantCultureIgnoreCase) && r.Value.RateLimiterPolicy != null && r.Value.RateLimiterPolicy.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase));

            //rate limit policy matching
            if (string.IsNullOrEmpty(ratePolicyName) || string.IsNullOrEmpty(routeMatch.Value.RateLimiterPolicy))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Missing";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync("Rate Limit Policy is missing for this request.", context.RequestAborted);
                return;
            }

            if (!routeMatch.Value.RateLimiterPolicy.Equals(ratePolicyName, StringComparison.InvariantCultureIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Policy_Rate_Limit_Invalid";
                context.Response.Headers.RetryAfter = "0";
                await context.Response.WriteAsync($"Rate Limit Policy is unknown for this request. Policy: '{ratePolicyName}'", context.RequestAborted);
                return;
            }

            // endpoint matching
            if ((routeMatch.Key == null || path == null) && routeMatch.Value.Match != null && routeMatch.Value.Match.Path != null && !routeMatch.Value.Match.Path.Equals(path))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Route_Path_Not_Found";
                await context.Response.WriteAsync($"Route is not found for this request. Route: '{path}'", context.RequestAborted);
                return;
            }

            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (!_rateLimitService.IsRequestAllowed(ratePolicyName, clientId))
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
                context.Response.Headers["X-Gateway-Error-Type"] = "Rate_Limit_Policy_Exceed";
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
                return;
            }

            var cacheKey = GenerateCacheKey(context, policy, clientId);

            if (_cache.TryGetValue(cacheKey, out byte[] cachedResponse))
            {
                _logger.LogDebug($"Fetch from gateway cache. client: {clientId}");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(JsonSerializer.Serialize(cachedResponse), context.RequestAborted);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            var headers = context.Request.Headers;

            headers["X-Forwarded-For"] = context.Connection.RemoteIpAddress?.ToString();
            headers["X-Forwarded-Host"] = context.Request.Host.Value;
            headers["X-Forwarded-Proto"] = context.Request.Scheme;
            headers["X-Forwarded-PathBase"] = context.Request.PathBase.Value ?? string.Empty;
            headers["X-Forwarded-Method"] = context.Request.Method;
            headers["X-Forwarded-Query"] = context.Request.QueryString.Value ?? string.Empty;
            headers["X-Forwarded-Path"] = context.Request.Path.Value ?? string.Empty;

            await _next(context); // Proceed to next middleware

            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            // Save to cache
            _cache.Set(cacheKey, responseBody, policy.Duration);

            // Reset stream and copy to original response
            memoryStream.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBodyStream;
            await memoryStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ReverseProxyServiceMiddleware.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error.");
        }

    }

    private string GenerateCacheKey(HttpContext context, OutputCachePolicy policy, string clientId)
    {
        var keyBuilder = new StringBuilder($"{context.Request.Path}:{clientId}:{context.Request.Headers.UserAgent}");
        return keyBuilder.ToString();
    }
}
