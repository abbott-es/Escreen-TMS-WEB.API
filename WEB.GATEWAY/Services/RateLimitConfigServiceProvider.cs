using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Services;

public sealed class RateLimitConfigServiceProvider : IRateLimitConfigServiceProvider, IRateLimitService
{
    private volatile InMemoryConfig _config;
    private readonly IMemoryCache _cache;
    private readonly object _lock = new();

    public RateLimitConfigServiceProvider(IOptionsMonitor<RateLimitingConfig> options, IMemoryCache cache)
    {
        _cache = cache;
        _config = BuildConfig(options.CurrentValue);
        options.OnChange(updated =>
        {
            lock (_lock)
            {
                _config = BuildConfig(updated);
                _config.SignalChange();
            }
        });
    }

    public async Task<bool> IsRequestAllowed(string policyName, string clientId, string req)
    {
        if (!_config.Policies.TryGetValue(policyName, out RateLimitOptions? policy))
            return false;

        string key = $"{policy.PolicyName.GetHashCode()}:{policy.PermitLimit.GetHashCode()}:{clientId.GetHashCode()}:{req.GetHashCode()}";
        

        List<DateTime>? timestamps = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(policy.WindowSeconds);
            return new List<DateTime>();
        });


        var now = DateTime.UtcNow;
        lock (timestamps) // Ensure thread safety
        {
            timestamps.RemoveAll(t => (now - t).TotalSeconds > policy.WindowSeconds);

            if (timestamps.Count >= policy.PermitLimit)
                return false;

            timestamps.Add(now);
        }

        return true;
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