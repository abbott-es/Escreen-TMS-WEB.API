using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEB.GATEWAY.Interfaces;
using WEB.UTILITY.Logger;

namespace Web.Gateway.Middleware;

/// <summary>
/// Middleware for handling reverse proxy routing and rate limiting.
/// </summary>
public class ReverseProxyServiceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IProxyConfigService _proxyConfigService;
    private readonly IRateLimitConfigServiceProvider _rateLimitService;
    private readonly IAppLogger<ReverseProxyServiceMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReverseProxyServiceMiddleware"/> class.
    /// </summary>
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

    /// <summary>
    /// Processes the HTTP request for reverse proxy and rate limiting.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // Validate required services
        if (_proxyConfigService == null)
        {
            await ThrowInternalServerError(context, nameof(IProxyConfigService));
            return;
        }
        if (_rateLimitService == null)
        {
            await ThrowInternalServerError(context, nameof(IRateLimitConfigServiceProvider));
            return;
        }

        // Get route for the current request path/method
        var routes = await _proxyConfigService.GetRoutesAsync();
        if (routes is null || routes.Count == 0)
        {
            await RespondWithError(context, StatusCodes.Status503ServiceUnavailable, "No routes configured in the proxy configuration.");
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        var route = routes.FirstOrDefault(r =>
            !string.IsNullOrEmpty(r.Match.Path) &&
            path.StartsWith(r.Match.Path!, StringComparison.OrdinalIgnoreCase) &&
            (r.Match.Methods == null || r.Match.Methods.Contains(method, StringComparer.OrdinalIgnoreCase)));

        if (route == null)
        {
            await RespondWithError(context, StatusCodes.Status404NotFound, "Route not found");
            return;
        }

        // Get cluster for the matched route
        var clusters = await _proxyConfigService.GetClustersAsync();
        if (clusters is null || clusters.Count == 0)
        {
            await RespondWithError(context, StatusCodes.Status503ServiceUnavailable, "No cluster services available.");
            return;
        }

        // Determine rate limit policy for the route
        string rateLimitPolicy = string.Empty;
        if (route.Metadata != null && route.Metadata.TryGetValue("RateLimitPolicy", out var policyValue))
        {
            rateLimitPolicy = policyValue ?? "blocked";
        }

        var rateLimit = _rateLimitService.GetPolicy(rateLimitPolicy);

        // Set rate limit headers only if a valid policy is found
        if (rateLimit != null)
        {
            context.Response.Headers["X-RateLimit-Limit"] = rateLimit.PermitLimit.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(rateLimit.WindowSeconds).ToUnixTimeSeconds().ToString();
            context.Response.Headers["X-RateLimit-Policy"] = rateLimit.PolicyName;
            _logger.LogDebug("Applying rate limit policy '{Policy}' for route '{RouteId}'", rateLimit.PolicyName, route.RouteId);
        }

        // If the policy is "blocked", short-circuit with 429
        if (rateLimit is { PolicyName: "blocked" })
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.AddSeconds(rateLimit.WindowSeconds).ToUnixTimeSeconds().ToString();
            await context.Response.WriteAsync("Rate limit blocked. Try again later.");
            return;
        }

        // Continue to next middleware
        await _next(context);
    }

    private async Task ThrowInternalServerError(HttpContext context, string missingService)
    {
        var exceptionMsg = $"{missingService} is not available in the request services.";
        _logger.LogError(new SystemException(exceptionMsg), exceptionMsg);
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("Internal server error.");
    }

    private async Task RespondWithError(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        _logger.LogError(new SystemException(message), message);
        await context.Response.WriteAsync(message);
    }
}
