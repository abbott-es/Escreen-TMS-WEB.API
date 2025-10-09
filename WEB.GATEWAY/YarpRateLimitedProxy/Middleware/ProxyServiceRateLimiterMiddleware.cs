using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.Configuration;
using YarpRateLimitedProxy.Services;

namespace YarpRateLimitedProxy.Middleware
{
    public class ProxyServiceRateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IProxyConfigService _proxyConfigService;
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<ProxyServiceRateLimiterMiddleware> _logger;

        public ProxyServiceRateLimiterMiddleware(
            RequestDelegate next,
            IProxyConfigService proxyConfigService,
            IRateLimitService rateLimitService,
            ILogger<ProxyServiceRateLimiterMiddleware> logger)
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

            var route = _proxyConfigService.Current.Routes.FirstOrDefault(r =>
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

    public class RateLimiterPolicyMetadata : IRateLimiterPolicyMetadata
    {
        public string PolicyName { get; }

        public RateLimiterPolicyMetadata(string policyName)
        {
            PolicyName = policyName;
        }
    }
}
