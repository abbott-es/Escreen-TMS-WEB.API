using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Interfaces
{
    public interface IProxyConfigService
    {
        /// <summary>
        /// Get Config Routes
        /// </summary>
        /// <returns>List of Route Config</returns>
        IReadOnlyList<RouteConfig> GetRoutes();
        /// <summary>
        /// Get Config Clusters
        /// </summary>
        /// <returns>List of Cluster Config</returns>
        IReadOnlyList<ClusterConfig> GetClusters();
    }
}
