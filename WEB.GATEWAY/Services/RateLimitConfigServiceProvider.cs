using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Threading;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Services;

internal sealed class RateLimitConfigServiceProvider : IRateLimitConfigServiceProvider
{
    private volatile InMemoryConfig _config;
    public RateLimitConfigServiceProvider(IOptionsMonitor<RateLimitConfig> options)
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

    public RateLimitOptions? GetPolicy(string policyName)
    {
        return _config.Policies.TryGetValue(policyName, out var policy) ? policy : null;
    }

    public IReadOnlyDictionary<string, RateLimitOptions> GetAllPolicies()
    {
        return _config.Policies;
    }

    private static InMemoryConfig BuildConfig(RateLimitConfig data)
    {
        return new InMemoryConfig(data);
    }

    /// <summary>
    /// Implements store for RateLimiter Config
    /// </summary>
    private class InMemoryConfig : IRateLimitConfig
    {
        public Dictionary<string, RateLimitOptions> Policies { get; set; }
        private CancellationTokenSource _cts = new();

        public InMemoryConfig(RateLimitConfig config)
        {
            Policies = config.Policies;
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