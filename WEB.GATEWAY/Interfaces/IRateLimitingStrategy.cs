using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Interfaces;

public interface IRateLimitingStrategy
{
    /// <summary>
    /// Enforce rate limiting based on the provided route configuration.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="routeConfig">The route configuration.</param>
    /// <returns>True if the request is allowed; otherwise, false.</returns>
    Task<bool> EnforceAsync(HttpContext context, RouteModel routeConfig);
}
