using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class PolicyBasedRateLimitingStrategy : IRateLimitingStrategy
{
    private readonly RateLimiterOptions _options;
    private readonly RateLimitingConfig _config;

    /// <summary>
    /// Holds rate limiters for different policies.
    /// </summary>
    /// <param name="options">Rate Limit options</param>
    /// <param name="config">Rate limiting configuration</param>
    public PolicyBasedRateLimitingStrategy(IOptions<RateLimiterOptions> options, IOptionsSnapshot<RateLimitingConfig> config)
    {
        _options = options.Value;
        _config = config.Value;
    }

    public async Task<bool> EnforceAsync(HttpContext context, RouteModel routeConfig)
    {
        var policyName = routeConfig?.Config.Metadata?.GetValueOrDefault(Constants.RATE_LIMIT_POLICY_METADATA_KEY);

        if (string.IsNullOrEmpty(policyName))
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "text/plain";
            context.Response.Headers.RetryAfter = "0";
            await context.Response.WriteAsync("Rate limit policy is missing for this request.", context.RequestAborted);
            return false;
        }

        if (!_config.Policies.TryGetValue(policyName, out var options))
        {
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            context.Response.ContentType = "text/plain";
            context.Response.Headers.RetryAfter = "0";
            await context.Response.WriteAsync($"Unknown rate limit policy: '{policyName}'", context.RequestAborted);
            return false;
        }

        _options.OnRejected = async (ctx, token) =>
        {

            ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            ctx.HttpContext.Response.ContentType = "text/plain";
            await ctx.HttpContext.Response.WriteAsync("Rate limit exceeded for this request.", token);
        };

        return true;
    }
}
