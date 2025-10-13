using System;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Interfaces;

public interface IRoutingStrategy
{
    /// <summary>
    /// Selects the destination based on the provided reverse proxy feature.
    /// </summary>
    /// <param name="feature">Current configuration on current request</param>
    /// <returns>Cluster destination state</returns>
    ValueTask<DestinationState?> SelectDestinationAsync(ClusterConfig feature);
}
