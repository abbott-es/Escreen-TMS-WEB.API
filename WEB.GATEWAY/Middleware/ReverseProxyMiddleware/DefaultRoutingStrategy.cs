using System;
using System.Linq;
using System.Threading.Tasks;
using WEB.GATEWAY.Interfaces;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

/// <summary>
/// Default routing strategy that selects the first available destination.
/// </summary>
public class DefaultRoutingStrategy : IRoutingStrategy
{

    public ValueTask<DestinationState?> SelectDestinationAsync(ClusterConfig cluster)
    {
        if (cluster?.Destinations == null || cluster.Destinations.Count == 0)
        {
            return ValueTask.FromResult<DestinationState?>(null);
        }

        var random = new Random();
        var position = random.Next(cluster.Destinations.Count);

        // Get destination ID and config
        var destId = cluster.Destinations.Keys.ElementAt(position);
        var destConfig = cluster.Destinations.Values.ElementAt(position);

        // Create DestinationState from config
        //CreateDestinationState
        //var destinationState = new DestinationState(destId)
        //{
        //    Model = new DestinationModel(destConfig)
        //};

        return ValueTask.FromResult<DestinationState?>(null);
    }

    private DestinationState CreateDestinationState(string destinationId, DestinationConfig config)
    {
        return new DestinationState(destinationId);
    }


}
