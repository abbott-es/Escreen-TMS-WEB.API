using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Models;

public class ReverseProxy
{
    public List<RouteConfig> Routes { get; set; } = new();
    public List<ClusterConfig> Clusters { get; set; } = new();
}