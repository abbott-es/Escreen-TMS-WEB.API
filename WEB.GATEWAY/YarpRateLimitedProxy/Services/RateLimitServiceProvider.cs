using System.Collections.Generic;
using Microsoft.Extensions.Options;
using YarpRateLimitedProxy.Models;

namespace YarpRateLimitedProxy.Services
{
    public class RateLimitServiceProvider : IRateLimitService
    {
        private volatile RateLimitingConfig _current;

        public RateLimitServiceProvider(IOptionsMonitor<RateLimitingConfig> options)
        {
            _current = options.CurrentValue;

            options.OnChange(updated =>
            {
                _current = updated;
            });
        }

        public RateLimitPolicyConfig? GetPolicy(string policyName)
        {
            return _current.Policies.TryGetValue(policyName, out var policy) ? policy : null;
        }

        public IReadOnlyDictionary<string, RateLimitPolicyConfig> GetAllPolicies()
        {
            return _current.Policies;
        }
    }
}
