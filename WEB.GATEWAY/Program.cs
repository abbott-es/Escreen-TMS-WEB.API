using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.ResponseCaching;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Middleware.ReverseProxyMiddleware;
using WEB.GATEWAY.Models;
using WEB.GATEWAY.Services;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

// Load configuration and create builder
var builder = WebApplication.CreateBuilder(args);
// Load configuration files
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
// Load environment-specific configuration if it exists
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
// Setup Logger
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();

builder.Host.UseSerilog();
Log.ForContext<Program>().Information(
    "API Gateway Initialized in {Environment} environment {Date} on {Urls}",
    builder.Environment.EnvironmentName,
    DateTime.UtcNow.ToUniversalTime(),
    builder.Configuration.GetValue<string>("ASPNETCORE_URLS") ?? "unset"
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

// Setup Reverse Proxy
Log.ForContext<Program>().Information("Setting up Reverse Proxy");

builder.Services.Configure<ReverseProxy>(builder.Configuration.GetSection(WEB.GATEWAY.Constants.REVERSE_PROXY));

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection(WEB.GATEWAY.Constants.REVERSE_PROXY));
//.AddTransforms(builderContext =>
//{
//    builderContext.AddRequestTransform(transformContext =>
//    {
//        var context = transformContext.HttpContext;
//        var headers = transformContext.ProxyRequest.Headers;

//        // Safely get values
//        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
//        var host = context.Request.Host.Value;
//        var scheme = context.Request.Scheme;
//        var pathBase = context.Request.PathBase.Value ?? string.Empty;
//        var method = context.Request.Method;
//        var query = context.Request.QueryString.Value ?? string.Empty;
//        var path = context.Request.Path.Value ?? string.Empty;

//        // Add headers only if not already present
//        if (!string.IsNullOrEmpty(remoteIp) && !headers.Contains("X-Forwarded-For"))
//        {
//            headers.TryAddWithoutValidation("X-Forwarded-For", remoteIp);
//        }

//        if (!headers.Contains("X-Forwarded-Host"))
//        {
//            headers.TryAddWithoutValidation("X-Forwarded-Host", host);
//        }

//        if (!headers.Contains("X-Forwarded-Proto"))
//            headers.TryAddWithoutValidation("X-Forwarded-Proto", scheme);

//        if (!headers.Contains("X-Forwarded-PathBase"))
//            headers.TryAddWithoutValidation("X-Forwarded-PathBase", pathBase);

//        if (!headers.Contains("X-Forwarded-Method"))
//            headers.TryAddWithoutValidation("X-Forwarded-Method", method);

//        if (!headers.Contains("X-Forwarded-Scheme"))
//            headers.TryAddWithoutValidation("X-Forwarded-Scheme", scheme);

//        if (!headers.Contains("X-Forwarded-Query"))
//            headers.TryAddWithoutValidation("X-Forwarded-Query", query);

//        if (!headers.Contains("X-Forwarded-Path"))
//            headers.TryAddWithoutValidation("X-Forwarded-Path", path);

//        return ValueTask.CompletedTask;
//    });
//});

builder.Services.AddSingleton<HttpMessageInvoker>(sp =>
{
    var handler = new SocketsHttpHandler
    {
        AllowAutoRedirect = false,
        AutomaticDecompression = DecompressionMethods.None,
        UseCookies = false
    };
    return new HttpMessageInvoker(handler);
});

builder.Services.AddSingleton(new ForwarderRequestConfig
{
    ActivityTimeout = TimeSpan.FromSeconds(360)
});

builder.Services.Configure<OutputCacheOptions>(builder.Configuration.GetSection("OutputCache"));
builder.Services.AddSingleton<IOutputCacheService, OutputCacheService>();
builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
builder.Services.AddMemoryCache();
// Setup Rate RateLiming Policies
builder.Services.AddRateLimiter(options =>
{
    var rateLimitConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfig>();

    if (rateLimitConfig?.Policies != null)
    {
        Log.ForContext<Program>().Information("Configuring Rate Limiting policies");
        foreach (var (policyName, policy) in rateLimitConfig.Policies)
        {
            options.AddPolicy(policyName, context =>
            {
                context.Response.Headers.RetryAfter = policy.WindowSeconds.ToString();
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Request.Path.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = policy.PermitLimit,
                        Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                        QueueLimit = policy.QueueLimit,
                        QueueProcessingOrder =
                            Enum.TryParse<QueueProcessingOrder>(policy.QueueProcessingOrder, ignoreCase: true,
                               out var queueProcessingOrder)
                                ? queueProcessingOrder
                                : QueueProcessingOrder.OldestFirst
                    });
            });
        }
    }
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetNoLimiter("NoRateLimitingPolicy");
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain";
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded for this request.", token);
    };
});
builder.Services.AddSingleton<IRateLimitConfigServiceProvider, RateLimitConfigServiceProvider>();
builder.Services.AddSingleton<IProxyConfigService, ProxyConfigServiceProvider>();
builder.Services.AddSingleton<IRoutingStrategy, DefaultRoutingStrategy>();
builder.Services.AddSingleton<IRateLimitingStrategy, PolicyBasedRateLimitingStrategy>();
builder.Services.AddSingleton<IForwardingStrategy, YarpForwardingStrategy>();
builder.Services.AddSingleton<IErrorHandlingStrategy, DefaultErrorHandlingStrategy>();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRateLimiter();

// Use external method to configure proxy pipeline
app.MapReverseProxy(UseProxyPipeline());

await app.RunAsync();
await Log.CloseAndFlushAsync();
return;

Action<IReverseProxyApplicationBuilder> UseProxyPipeline()
{
    async Task CustomProxyMiddleware(HttpContext context, RequestDelegate next)
    {
        await next(context); // Continue to next middleware (YARP)
    }
    return proxy =>
    {
        proxy.Use(CustomProxyMiddleware);
        proxy.UseMiddleware<ReverseProxyCacheMiddleware>();
        proxy.UseMiddleware<ReverseProxyServiceMiddleware>();
    };
}