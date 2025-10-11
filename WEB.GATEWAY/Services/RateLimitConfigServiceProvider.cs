using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Threading;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Services;

public sealed class RateLimitConfigServiceProvider : IRateLimitConfigServiceProvider
{
    private volatile InMemoryConfig _config;
    public RateLimitConfigServiceProvider(IOptionsMonitor<RateLimitingConfig> options)
    {
        _config = BuildConfig(options.CurrentValue);
        options.OnChange(updated =>
        {
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

    private static InMemoryConfig BuildConfig(RateLimitingConfig data)
    {
        return new(data);
    }

    /// <summary>
    /// Implements store for RateLimiter Config
    /// </summary>
    private sealed class InMemoryConfig(RateLimitingConfig config) : IRateLimitConfig
    {
        public IReadOnlyDictionary<string, RateLimitOptions> Policies { get; } = config.Policies != null
                ? new Dictionary<string, RateLimitOptions>(config.Policies)
                : [];
        private CancellationTokenSource _cts = new();

        public IChangeToken ChangeToken => new CancellationChangeToken(_cts.Token);

        public void SignalChange()
        {
            var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            previousCts.Cancel();
        }
    }
}