using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Interfaces;

public interface IForwardingStrategy
{
    /// <summary>
    /// Forwards the request to the specified destination.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="destination">The destination state.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask<ForwarderError> ForwardAsync(HttpContext context, DestinationState destination);
}
