using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace YarpRateLimitedProxy.Models
{
    public class ProxyConfigData
    {
        public List<RouteConfig> Routes { get; set; } = new();
        public List<ClusterConfig> Clusters { get; set; } = new();
    }
}
