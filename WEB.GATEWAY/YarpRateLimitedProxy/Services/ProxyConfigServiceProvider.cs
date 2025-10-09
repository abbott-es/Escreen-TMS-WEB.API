using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using YarpRateLimitedProxy.Models;

namespace YarpRateLimitedProxy.Services
{
    public class ProxyConfigServiceProvider : IProxyConfigProvider, IProxyConfigService
    {
        private volatile InMemoryConfig _config;
        private ProxyConfigData _current;

        public ProxyConfigServiceProvider(IOptionsMonitor<ProxyConfigData> options)
        {
            _current = options.CurrentValue;
            _config = BuildConfig(_current);

            options.OnChange(updated =>
            {
                _current = updated;
                _config = BuildConfig(updated);
                _config.SignalChange();
            });
        }

        public IProxyConfig GetConfig() => _config;

        public ProxyConfigData Current => _current;

        private InMemoryConfig BuildConfig(ProxyConfigData data)
        {
            return new InMemoryConfig(data.Routes, data.Clusters);
        }

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

            public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);

            public void SignalChange()
            {
                var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                previousCts.Cancel();
            }
        }
    }
}
