using System.Collections.Generic;
using WEB.GATEWAY.Interfaces;

namespace WEB.GATEWAY.Models;

public class RateLimitConfig : IRateLimitConfig
{
    public IReadOnlyDictionary<string, RateLimitOptions> Policies { get; set; } = new Dictionary<string, RateLimitOptions>();
}