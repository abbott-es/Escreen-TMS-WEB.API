using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Logger;

namespace Web.Gateway.Middleware;

public class ReverseProxyServiceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProxyConfigService _proxyConfigService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IAppLogger<ReverseProxyServiceMiddleware> _logger;

    public ReverseProxyServiceMiddleware(
        RequestDelegate next,
        IProxyConfigService proxyConfigService,
        IRateLimitConfigServiceProvider rateLimitService,
        IAppLogger<ReverseProxyServiceMiddleware> logger)
    {
        _next = next;
        _proxyConfigService = proxyConfigService;
        _rateLimitService = rateLimitService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.Request.Method;
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       
        var route = _proxyConfigService.GetRoutes()
            .FirstOrDefault(r =>
                r.Match?.Path is string matchPath &&
                path.StartsWith(matchPath.TrimEnd('*'), StringComparison.OrdinalIgnoreCase) &&
                (r.Match.Methods == null || r.Match.Methods.Contains(method, StringComparer.OrdinalIgnoreCase)));
        string? rateLimitPolicy = null;
        
        route?.Metadata?.TryGetValue("RateLimitPolicy", out rateLimitPolicy);

        RateLimitOptions? rateLimit = _rateLimitService.GetPolicy(rateLimitPolicy ?? "blocked");
        context.Response.Headers["X-RateLimit-Limit"] = rateLimit?.PermitLimit.ToString() ?? "0";
        context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow
            .Add(TimeSpan.FromSeconds(rateLimit.WindowSeconds)).ToUnixTimeSeconds().ToString();

        if (rateLimit is { PolicyName: "blocked" })
        {
            await context.Response.WriteAsync("Rate limit blocked. Try again later.");
        }

        if (rateLimitPolicy is not null && rateLimit is not null)
        {
            _logger.LogDebug("Applying rate limit policy '{Policy}' for route '{RouteId}'", rateLimitPolicy, route?.RouteId!);

        }

        // Functional-style loading of headers, query strings, and form parameters
        var allParams = new Dictionary<string, string>();

        context.Request.Headers
            .ToList()
            .ForEach(header => allParams[$"Header:{header.Key}"] = header.Value.ToString());

        context.Request.Query
            .ToList()
            .ForEach(query => allParams[$"Query:{query.Key}"] = query.Value.ToString());

        if (context.Request.HasFormContentType)
        {
            var form = await context.Request.ReadFormAsync();
            form.ToList()
                .ForEach(param => allParams[$"Form:{param.Key}"] = param.Value.ToString());
        }

        // Store all collected parameters in HttpContext.Items
        context.Items["RequestPayload"] = allParams;
        

        await _next(context);
    }
}

/// <summary>
/// Marker interface for rate limiter policy metadata.
/// </summary>
public interface IRateLimiterPolicyMetadata
{
    string PolicyName { get; }
}

public class RateLimiterPolicyMetadata : IRateLimiterPolicyMetadata
{
    public string PolicyName { get; }

    public RateLimiterPolicyMetadata(string policyName)
    {
        PolicyName = policyName;
    }
}
