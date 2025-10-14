using System.Collections.Generic;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Interfaces;

public interface IRateLimitConfigServiceProvider
{
    /// <summary>
    /// Get one RateLimit policy
    /// </summary>
    /// <param name="policyName">Name of the Rate Limit Policy</param>
    /// <returns></returns>
    RateLimitOptions? GetPolicy(string policyName);
    /// <summary>
    /// Get All RateLimit Policy
    /// </summary>
    /// <returns>Dictionary data of all Rate Limit Policy</returns>
    IReadOnlyDictionary<string, RateLimitOptions> GetAllPolicies();
    /// <summary>
    /// Check if Request allowable for that Policy and agent
    /// </summary>
    /// <param name="policyName"></param>
    /// <param name="clientId"></param>
    /// <returns></returns>
    bool IsRequestAllowed(string policyName, string clientId);
}