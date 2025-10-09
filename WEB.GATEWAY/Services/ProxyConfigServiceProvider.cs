using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WEB.GATEWAY.Models;
using Yarp.ReverseProxy.Configuration;

namespace WEB.GATEWAY.Services
{
    /// <summary>
    /// Implementing a Reverse Proxy Config Service Provider for dynamic proxy setup
    /// </summary>
    internal sealed class ProxyConfigServiceProvider : IProxyConfigProvider, Interfaces.IProxyConfigService
    {
        private volatile InMemoryConfig _config;

        public ProxyConfigServiceProvider(IOptionsMonitor<ReverseProxy> options)
        {
            var current = options.CurrentValue;
            _config = BuildConfig(current);

            options.OnChange(updated =>
            {
                current = updated;
                _config = BuildConfig(updated);
                _config.SignalChange();
            });
        }

        public IProxyConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// Get Routes from Proxy Config Service Provider
        /// </summary>
        /// <returns>Read only list of Routes</returns>
        public IReadOnlyList<RouteConfig> GetRoutes()
        {
            return _config.Routes;
        }

        /// <summary>
        /// Get Clusters from Proxy Config Service Provider
        /// </summary>
        /// <returns>Read only list of Clusters</returns>
        public IReadOnlyList<ClusterConfig> GetClusters()
        {
            return _config.Clusters;
        }

        /// <summary>
        /// Replace the old config with updated config
        /// </summary>
        /// <param name="data">Bind Reverse Proxy data</param>
        /// <returns>Updated Config InMemoryConfig ReverseProxy</returns>
        private static InMemoryConfig BuildConfig(ReverseProxy data)
        {
            return new InMemoryConfig(data.Routes, data.Clusters);
        }
        
        /// <summary>
        /// Implements store for Routes and Clusters
        /// </summary>
        private class InMemoryConfig : IProxyConfig
        {
            public IReadOnlyList<RouteConfig> Routes { get; }
            public IReadOnlyList<ClusterConfig> Clusters { get; }
            private CancellationTokenSource _cts = new();

            public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes;
                Clusters = clusters;
            }

            public IChangeToken ChangeToken
            {
                get { return new CancellationChangeToken(_cts.Token); }
            }

            public void SignalChange()
            {
                var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                previousCts.Cancel();
            }
        }

    }
}
