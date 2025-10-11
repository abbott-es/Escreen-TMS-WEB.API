using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEB.GATEWAY.Interfaces;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Forwarder;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class ReverseProxyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRoutingStrategy _routingStrategy;
    private readonly IRateLimitingStrategy _rateLimitingStrategy;
    private readonly IErrorHandlingStrategy _errorHandlingStrategy;
    private readonly IForwardingStrategy _forwardingStrategy;
    private readonly IAppLogger<ReverseProxyMiddleware> _logger;

    public ReverseProxyMiddleware(
        RequestDelegate next,
        IRoutingStrategy routingStrategy,
        IRateLimitingStrategy rateLimitingStrategy,
        IErrorHandlingStrategy errorHandlingStrategy,
        IForwardingStrategy forwardingStrategy,
        IAppLogger<ReverseProxyMiddleware> logger)
    {
        _next = next;
        _routingStrategy = routingStrategy;
        _rateLimitingStrategy = rateLimitingStrategy;
        _errorHandlingStrategy = errorHandlingStrategy;
        _forwardingStrategy = forwardingStrategy;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var proxyFeature = context.Features.Get<Yarp.ReverseProxy.Model.IReverseProxyFeature>();

        if (proxyFeature == null)
        {
            _logger.LogDebug("ReverseProxyFeature is missing in the HttpContext. Skipping proxying.");
            await _next(context);
            _logger.LogDebug("Returned from next middleware.");
            return;
        }

        var destination = proxyFeature.ProxiedDestination ?? await _routingStrategy.SelectDestinationAsync(proxyFeature);

        if (destination == null)
        {
            _logger.LogDebug("No destination found for {Route}.", proxyFeature.Route.Config.ToString());
            await _errorHandlingStrategy.HandleMissingDestinationAsync(context);
            return;
        }

        var allowed = await _rateLimitingStrategy.EnforceAsync(context, proxyFeature.Route);

        if (!allowed)
        {
            _logger.LogDebug($"Rate limit policy: {proxyFeature.Route.Config.Metadata?.GetValueOrDefault(Constants.RATE_LIMIT_POLICY_METADATA_KEY) ?? "unknown"} is not allowed for {proxyFeature.Route.Config.Match.Path}.");
            return;
        }

        var error = await _forwardingStrategy.ForwardAsync(context, destination);

        if (error != ForwarderError.None)
        {
            await _errorHandlingStrategy.HandleForwardingErrorAsync(context, error);
        }
    }
}
