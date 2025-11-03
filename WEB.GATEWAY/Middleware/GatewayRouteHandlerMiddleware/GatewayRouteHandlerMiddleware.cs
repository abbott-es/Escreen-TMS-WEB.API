using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.UTILITY.Logger;

namespace WEB.GATEWAY.Middleware.GatewayRouteHandlerMiddleware;

public class GatewayRouteHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly GatewaySettings _settings;
    private readonly IAppLogger<GatewayRouteHandlerMiddleware> _appLogger;
    private readonly IProxyConfigService _routing;
    public GatewayRouteHandlerMiddleware(RequestDelegate request,
        IOptions<GatewaySettings> settings,
        IProxyConfigService proxyConfigService,
        IAppLogger<GatewayRouteHandlerMiddleware> logger)
    {
        _next = request;
        _settings = settings.Value;
        _routing = proxyConfigService;
        _appLogger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.HasValue && ((IEnumerable<string>)["PUT", "POST"]).Contains(context.Request.Method))
        {
            var route = await _routing.GetRoutesAsync();
            var matchRoute = route.Find(e => e.Value.Match.Path == context.Request.Path);
            _appLogger.LogDebug($"MatchRoute: {matchRoute}");
           if(matchRoute.IsNone)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                context.Response.ContentType = "text/plain";
                context.Response.Headers["X-Gateway-Error-Type"] = "Gateway_Route_Not_Available";
                await context.Response.WriteAsync("Gateway Service Not Available for this request.", context.RequestAborted);
                return;
            }
           if(matchRoute.IsSome)
            {
                // Transformation here if there is a match current no need to implement since no custom logic.
            }
        }
        await _next(context);
    }
}
