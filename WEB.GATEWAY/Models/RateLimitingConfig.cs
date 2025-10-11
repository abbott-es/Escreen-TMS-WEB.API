using System.Collections.Generic;
using WEB.GATEWAY.Interfaces;

namespace WEB.GATEWAY.Models;

/// <summary>
/// Configuration for rate limiting policies.
/// </summary>
public class RateLimitingConfig : IRateLimitConfig
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, RateLimitOptions> Policies { get; set; } = new Dictionary<string, RateLimitOptions>();
}