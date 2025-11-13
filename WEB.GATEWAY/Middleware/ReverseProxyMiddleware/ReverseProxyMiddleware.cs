using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Caching;
using WEB.UTILITY.LanguageExt;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Middleware
{
    public class ReverseProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ICache _cache;
        private readonly IOutputCacheService _policyService;
        private readonly IRateLimitConfigServiceProvider _rateLimitService;
        private readonly IProxyConfigService _routing;
        private readonly IAppLogger<ReverseProxyMiddleware> _logger;
        private readonly IRateLimitingStrategy _rateLimitStrategy;
        private readonly IErrorHandlingStrategy _errorHandlingStrat;
        private readonly GatewaySettings _settings;

        public ReverseProxyMiddleware(
            RequestDelegate next,
            ICache cache,
            IOutputCacheService policyService,
            IRateLimitConfigServiceProvider rateLimitService,
            IProxyConfigService routingService,
            IRateLimitingStrategy rateLimitStrategy,
            IErrorHandlingStrategy errorHandlingStrategy,
            IOptions<GatewaySettings> settings,
            IAppLogger<ReverseProxyMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _policyService = policyService;
            _rateLimitService = rateLimitService;
            _routing = routingService;
            _rateLimitStrategy = rateLimitStrategy;
            _errorHandlingStrat = errorHandlingStrategy;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var proxyFeature = context.Features.Get<IReverseProxyFeature>();
            if (proxyFeature == null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Gateway unavailable.");
                return;
            }
            var originalBody = context.Response.Body;
            using var buffer = new MemoryStream();
            context.Response.Body = buffer;

            await _next(context);

            buffer.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(buffer).ReadToEndAsync();

            if (cacheable)
            {
                await _cache.Set(cacheKey, responseBody, TimeSpan.FromMinutes(1));
            }

            buffer.Seek(0, SeekOrigin.Begin);
            context.Response.Body = originalBody;
            await buffer.CopyToAsync(originalBody);
        }
    }
}
