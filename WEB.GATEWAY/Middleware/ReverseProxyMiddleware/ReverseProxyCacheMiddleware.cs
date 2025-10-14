using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class ReverseProxyCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly IOutputCacheService _policyService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IProxyConfigService _routingService;

    public ReverseProxyCacheMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        IOutputCacheService policyService,
        IRateLimitConfigServiceProvider rateLimitService,
        IProxyConfigService routingService)
    {
        _next = next;
        _cache = cache;
        _policyService = policyService;
        _rateLimitService = rateLimitService;
        _routingService = routingService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cachePolicyName = context.Request.Headers["X-Cache-Policy"].FirstOrDefault() ?? string.Empty;

        var policy = _policyService.GetPolicy(cachePolicyName);
        if (policy == null)
        {
            context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Invalid cache Policy for this request.", context.RequestAborted);
            return;
        }

        // endpoint matching
        var path = context.Request.Path;
        var routeMatch = (await _routingService.GetRoutesAsync()).Keys.FirstOrDefault(k =>
            k.Contains(path, StringComparison.InvariantCultureIgnoreCase));
        if (string.IsNullOrWhiteSpace(routeMatch))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Route not found for this request", context.RequestAborted);
            return;
        }

        // Rate limiting check
        var ratePolicyName = context.Request.Headers["X-RateLimit-Policy"].FirstOrDefault() ?? string.Empty;
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        if (!_rateLimitService.IsRequestAllowed(ratePolicyName, clientId))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Rate limit exceeded for this request.", context.RequestAborted);
            return;
        }

        var cacheKey = GenerateCacheKey(context, policy, clientId);
        if (_cache.TryGetValue(cacheKey, out var cachedResponse))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync((string)cachedResponse);
            return;
        }

        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        await _next(context); // GO TO THE NEXT MIDDLEWARE

        memoryStream.Seek(0, SeekOrigin.Begin);
        var responseBody = new StreamReader(memoryStream).ReadToEnd();
        _cache.Set(cacheKey, responseBody, policy.Duration);

        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalBodyStream);
    }

    private string GenerateCacheKey(HttpContext context, OutputCachePolicy policy, string clientId)
    {
        var keyBuilder = new StringBuilder($"{context.Request.Path}:{clientId}:{context.Request.Headers.UserAgent}");
        return keyBuilder.ToString();
    }
}
