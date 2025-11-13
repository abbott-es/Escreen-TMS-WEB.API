using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEB.GATEWAY.Interfaces;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Forwarder;

namespace WEB.GATEWAY.Middleware;

public class DefaultErrorHandlingStrategy : IErrorHandlingStrategy
{
    private readonly IAppLogger<DefaultErrorHandlingStrategy> _logger;

    public DefaultErrorHandlingStrategy(IAppLogger<DefaultErrorHandlingStrategy> logger)
    {
        _logger = logger;
    }

    public async Task HandleForwardingErrorAsync(HttpContext context, ForwarderError error)
    {
        var errorFeature = context.Features.Get<IForwarderErrorFeature>();
        _logger.LogError(errorFeature?.Exception!, $"Forwarding error: {Enum.GetName<ForwarderError>(error)}");
        context.Response.Headers["X-Gateway-Error-Type"] = Enum.GetName<ForwarderError>(error) ?? "Unknown_Error";
        await AddDetails(context);
    }

    public async Task HandleMissingDestinationAsync(HttpContext context)
    {
        context.Response.Headers["X-Gateway-Error-Type"] = "No_Destination_Found";
        await AddDetails(context);
    }
    private async static Task AddDetails(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status502BadGateway;
        context.Response.ContentType = "text/plain";
        context.Response.Headers.RetryAfter = "5"; // Suggests the client to retry after 5 seconds
        await context.Response.WriteAsync("Bad Gateway.", context.RequestAborted);
    }
}
