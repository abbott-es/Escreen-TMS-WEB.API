using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEB.GATEWAY.Interfaces;
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
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        var route = _proxyConfigService.GetRoutes().FirstOrDefault(r =>
            r.Match != null &&
            !string.IsNullOrEmpty(r.Match.Path) &&
            path.StartsWith(r.Match.Path.TrimEnd('*'), System.StringComparison.OrdinalIgnoreCase) &&
            (r.Match.Methods == null || r.Match.Methods.Contains(method, System.StringComparer.OrdinalIgnoreCase)));

        if (route?.Metadata != null &&
            route.Metadata.TryGetValue("RateLimitPolicy", out var policyName) &&
            _rateLimitService.GetPolicy(policyName) != null)
        {
            _logger.LogDebug("Applying rate limit policy '{Policy}' for route '{RouteId}'", policyName, route.RouteId);

            context.SetEndpoint(new Endpoint(
                async ctx => await _next(ctx),
                new EndpointMetadataCollection(new RateLimiterPolicyMetadata(policyName)),
                $"RateLimitedEndpoint:{route.RouteId}"
            ));
        }

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
