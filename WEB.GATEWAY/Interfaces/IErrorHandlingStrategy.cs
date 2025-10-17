using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;

namespace WEB.GATEWAY.Interfaces;

public interface IErrorHandlingStrategy
{
    /// <summary>
    /// Handles the scenario when no destination is found.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>Error response.</returns>
    Task HandleMissingDestinationAsync(HttpContext context);
    /// <summary>
    /// Handles forwarding errors.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="error">The forwarding error.</param>
    /// <returns>Error response.</returns>
    Task HandleForwardingErrorAsync(HttpContext context, ForwarderError error);
}
