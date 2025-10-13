using System;
using System.Collections.Generic;

namespace WEB.GATEWAY.Models;

public class OutputCachePolicy
{
    /// <summary>
    /// Length of Cache Lifetime
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Caching Options 
/// </summary>
public class OutputCacheOptions
{
    /// <summary>
    /// List of Cache Policies
    /// </summary>
    public IReadOnlyDictionary<string, OutputCachePolicy> Policies { get; set; } 
}
