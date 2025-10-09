using System.Collections.Generic;
using YarpRateLimitedProxy.Models;

namespace YarpRateLimitedProxy.Services
{
    public interface IRateLimitService
    {
        RateLimitPolicyConfig? GetPolicy(string policyName);
        IReadOnlyDictionary<string, RateLimitPolicyConfig> GetAllPolicies();
    }
}
