using System.Collections.Generic;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Interfaces;

public interface IProxyConfigService
{
    /// <summary>
    /// Get Config Routes
    /// </summary>
    /// <returns>List of Route Config</returns>
    Task<IReadOnlyDictionary<string, RouteConfig>> GetRoutesAsync();
    /// <summary>
    /// Get Config Clusters
    /// </summary>
    /// <returns>List of Cluster Config</returns>
    Task<IReadOnlyDictionary<string, ClusterConfig>> GetClustersAsync();
}
