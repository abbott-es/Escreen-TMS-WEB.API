using System;
using System.Linq;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

/// <summary>
/// Default routing strategy that selects the first available destination.
/// </summary>
public class DefaultRoutingStrategy : IRoutingStrategy
{
    public ValueTask<DestinationState?> SelectDestinationAsync(IReverseProxyFeature feature)
    {
        var destination = feature?.AvailableDestinations?.FirstOrDefault();
        return ValueTask.FromResult(destination);
    }
}
