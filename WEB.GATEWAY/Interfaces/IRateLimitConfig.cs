using System.Collections.Generic;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Interfaces;

public interface IRateLimitConfig
{
    /// <summary>
    /// Get all RateLimit Policy
    /// </summary>
    IReadOnlyDictionary<string, RateLimitOptions> Policies { get; }
}