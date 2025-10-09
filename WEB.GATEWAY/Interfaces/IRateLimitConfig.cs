using System.Collections.Generic;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Interfaces;

public interface IRateLimitConfig
{
    Dictionary<string, RateLimitOptions> Policies { get; set; }
}