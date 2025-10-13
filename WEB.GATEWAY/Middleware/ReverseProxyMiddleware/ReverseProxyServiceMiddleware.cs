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
    private readonly IProxyConfigService _service;
    private readonly IAppLogger<ReverseProxyMiddleware> _logger;

    public ReverseProxyMiddleware(
        RequestDelegate next,
        IRoutingStrategy routingStrategy,
        IRateLimitingStrategy rateLimitingStrategy,
        IErrorHandlingStrategy errorHandlingStrategy,
        IForwardingStrategy forwardingStrategy,
        IProxyConfigService service,
        IAppLogger<ReverseProxyMiddleware> logger)
    {
        _next = next;
        _routingStrategy = routingStrategy;
        _rateLimitingStrategy = rateLimitingStrategy;
        _errorHandlingStrategy = errorHandlingStrategy;
        _forwardingStrategy = forwardingStrategy;
        _service = service;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            _logger.LogDebug("Empty routes");
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
            _logger.LogDebug("NO matching routes");
            await _next(context);
        }

        var clusters = await _service.GetClustersAsync();

        var matchedCluster =clusters.Values.FirstOrDefault(c => c.ClusterId == matchedRoute?.ClusterId);
        if (matchedCluster == null)
        {
            _logger.LogDebug("No matching cluster for that route");
            await _next(context);
        }

        var destination = await _routingStrategy.SelectDestinationAsync(matchedCluster);

        if (destination == null)
        {
            _logger.LogDebug("No destination found for {Route}.", matchedRoute?.Match?.Path!);
            await _errorHandlingStrategy.HandleMissingDestinationAsync(context);
            return;
        }


      //  var allowed = await _rateLimitingStrategy.EnforceAsync(context, proxyFeature.Route);

        var error = await _forwardingStrategy.ForwardAsync(context, destination);

        if (error != ForwarderError.None)
        {
            await _errorHandlingStrategy.HandleForwardingErrorAsync(context, error);
        }
    }
}
