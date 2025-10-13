using System.Collections.Generic;
using Microsoft.Extensions.Options;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;

namespace WEB.GATEWAY.Services;

public class OutputCacheService : IOutputCacheService
{
    private readonly IReadOnlyDictionary<string, OutputCachePolicy> _policies;

    public OutputCacheService(IOptions<OutputCacheOptions> options)
    {
        _policies = options.Value.Policies;
    }

    public OutputCachePolicy? GetPolicy(string name)
    {
        _policies.TryGetValue(name, out var policy);
        return policy;
    }
}