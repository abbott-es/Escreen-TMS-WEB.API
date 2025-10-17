using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEB.GATEWAY.Interfaces;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;

namespace WEB.GATEWAY.Middleware.ReverseProxyMiddleware;

public class YarpForwardingStrategy : IForwardingStrategy
{
    private readonly IHttpForwarder _forwarder;
    private readonly HttpMessageInvoker _httpClient;
    private readonly ForwarderRequestConfig _config;

    public YarpForwardingStrategy(
        IHttpForwarder forwarder,
        HttpMessageInvoker httpClient,
        ForwarderRequestConfig config)
    {
        _forwarder = forwarder;
        _httpClient = httpClient;
        _config = config;
    }

    public ValueTask<ForwarderError> ForwardAsync(HttpContext context, DestinationState destination)
    {

        return _forwarder.SendAsync(context, destination.DestinationId, _httpClient, _config);
    }
}

