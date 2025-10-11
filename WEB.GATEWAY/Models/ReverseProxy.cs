using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Models;

public class ReverseProxy
{
    public IReadOnlyList<RouteConfig> Routes { get; set; } = [];
    public IReadOnlyList<ClusterConfig> Clusters { get; set; } = [];
}