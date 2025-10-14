using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Logger;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;
public class ReverseProxyServiceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRoutingStrategy _routingStrategy;
    private readonly IRateLimitingStrategy _rateLimitingStrategy;
    private readonly IErrorHandlingStrategy _errorHandlingStrategy;
    private readonly IForwardingStrategy _forwardingStrategy;
    private readonly IProxyConfigService _service;
    private readonly IMemoryCache _cache;
    private readonly IOutputCacheService _outputCacheService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IAppLogger<ReverseProxyServiceMiddleware> _logger;

    public ReverseProxyServiceMiddleware(
        RequestDelegate next,
        IRoutingStrategy routingStrategy,
        IRateLimitingStrategy rateLimitingStrategy,
        IErrorHandlingStrategy errorHandlingStrategy,
        IForwardingStrategy forwardingStrategy,
        IProxyConfigService service,
        IMemoryCache cache,
        IOutputCacheService outputCacheService,
        IRateLimitConfigServiceProvider rateLimitService,
        IAppLogger<ReverseProxyServiceMiddleware> logger)
    {
        _next = next;
        _routingStrategy = routingStrategy;
        _rateLimitingStrategy = rateLimitingStrategy;
        _errorHandlingStrategy = errorHandlingStrategy;
        _forwardingStrategy = forwardingStrategy;
        _service = service;
        _outputCacheService = outputCacheService;
        _cache = cache;
        _rateLimitService = rateLimitService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            //var proxyFeature = context.Features.Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();
            //var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            //if (proxyFeature == null)
            //{

            //}
            //var destination = proxyFeature.ProxiedDestination ?? await _routingStrategy.SelectDestinationAsync(proxyFeature);

            //if (destination == null)
            //{
            //    _logger.LogDebug("No destination found for {Route}.", proxyFeature.Route.Config.ToString());
            //    await _errorHandlingStrategy.HandleMissingDestinationAsync(context);
            //    return;
            //}


            // Add default YARP headers manually
            var headers = context.Request.Headers;

            headers["X-Forwarded-For"] = context.Connection.RemoteIpAddress?.ToString();
            headers["X-Forwarded-Host"] = context.Request.Host.Value;
            headers["X-Forwarded-Proto"] = context.Request.Scheme;
            headers["X-Forwarded-PathBase"] = context.Request.PathBase.Value ?? "";
            headers["X-Forwarded-Method"] = context.Request.Method;
            headers["X-Forwarded-Query"] = context.Request.QueryString.Value ?? "";
            headers["X-Forwarded-Path"] = context.Request.Path.Value ?? "";


            var cachePolicyName = context.Request.Headers["X-Cache-Policy"].FirstOrDefault() ?? "ShortTerm";
            var ratePolicyName = context.Request.Headers["X-RateLimit-Policy"].FirstOrDefault() ?? "balanced";
            var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Rate limiting check
            if (!_rateLimitService.IsRequestAllowed(ratePolicyName, clientId))
            {
                var retryAfter = _rateLimitService.GetPolicy(ratePolicyName)?.WindowSeconds.ToString() ?? "0";
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = retryAfter;
                await context.Response.WriteAsync("Rate limit exceeded.");
                _logger.LogWarning($"Rate limit exceeded for client {clientId} using policy {ratePolicyName}");
                return;
            }

            // Output caching check
            var cachePolicy = _outputCacheService.GetPolicy(cachePolicyName);
            if (cachePolicy != null)
            {
                var cacheKey = GenerateCacheKey(context, cachePolicy);
                if (_cache.TryGetValue(cacheKey, out var cachedResponse))
                {
                    _logger.LogDebug("Serving response from cache.");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync((string)cachedResponse);
                    return;
                }

                var originalBodyStream = context.Response.Body;
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                await _next(context); // Proceed to next middleware

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                _cache.Set(cacheKey, responseBody, cachePolicy.Duration);

                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            else
            {
                await _next(context);
            }

            // Routing logic
            await HandleRoutingAsync(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ReverseProxyServiceMiddleware.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error.");
        }
    }

    private async Task HandleRoutingAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogDebug("Empty request path.");
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        var routes = await _service.GetRoutesAsync();

        var matchedRoute = routes.Values.FirstOrDefault(r =>
            r.Match?.Path is string matchPath &&
            path.StartsWith(matchPath.TrimEnd('*'), StringComparison.OrdinalIgnoreCase) &&
            (r.Match.Methods == null || r.Match.Methods.Contains(method, StringComparer.OrdinalIgnoreCase))
        );

        if (matchedRoute == null)
        {
            _logger.LogDebug("No matching route found.");
            await _next(context);
            return;
        }

        var clusters = await _service.GetClustersAsync();
        var matchedCluster = clusters.Values.FirstOrDefault(c => c.ClusterId == matchedRoute.ClusterId);

        if (matchedCluster == null)
        {
            _logger.LogDebug("No matching cluster found for route.");
            await _next(context);
        }

    }

    private static string GenerateCacheKey(HttpContext context, OutputCachePolicy policy)
    {
        var keyBuilder = new StringBuilder($"{context.Request.Path}:{context.Request.Headers.UserAgent}:{context.Request.HttpContext.Connection.RemoteIpAddress}");
        return keyBuilder.ToString();
    }
}
