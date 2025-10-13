using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Services
{
    /// <summary>
    /// Implementing a Reverse Proxy Config Service Provider for dynamic proxy setup
    /// </summary>
    public sealed class ProxyConfigServiceProvider : IProxyConfigProvider, IProxyConfigService
    {
        private volatile InMemoryConfig _config;

        public ProxyConfigServiceProvider(IOptionsMonitor<ReverseProxy> options)
        {
            _config = BuildConfig(options.CurrentValue);

            options.OnChange(updated =>
            {
                _config = BuildConfig(updated);
                _config.SignalChange();
            });
        }
        
        public IProxyConfig GetConfig()
        {
            return null;
        }

        /// <summary>
        /// Get Routes from Proxy Config Service Provider
        /// </summary>
        public Task<IReadOnlyDictionary<string, RouteConfig>> GetRoutesAsync()
        {
            return Task.FromResult(_config.Routes);
        }

        /// <summary>
        /// Get Clusters from Proxy Config Service Provider
        /// </summary>
        public Task<IReadOnlyDictionary<string, ClusterConfig>> GetClustersAsync()
        {
            return Task.FromResult(_config.Clusters);
        }

        /// <summary>
        /// Replace the old config with updated config
        /// </summary>
        private static InMemoryConfig BuildConfig(ReverseProxy data)
        {
            // Defensive copy for immutability
            var routes = data.Routes != null && data.Routes.Count > 0
                ? new Dictionary<string, RouteConfig>(data.Routes)
                : new Dictionary<string, RouteConfig>();
            var clusters = data.Clusters != null && data.Clusters.Count > 0 ? new Dictionary<string, ClusterConfig>(data.Clusters) : new Dictionary<string, ClusterConfig>();
            return new InMemoryConfig(routes, clusters);
        }

        /// <summary>
        /// Implements store for Routes and Clusters
        /// </summary>
        private sealed class InMemoryConfig(IReadOnlyDictionary<string, RouteConfig> routes, IReadOnlyDictionary<string, ClusterConfig> clusters) 
        {
            public IReadOnlyDictionary<string, RouteConfig> Routes { get; } = routes;
            public IReadOnlyDictionary<string, ClusterConfig> Clusters { get; } = clusters;
            private CancellationTokenSource _cts = new();

            public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);

            public void SignalChange()
            {
                var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                previousCts.Cancel();
            }
        }
    }
}