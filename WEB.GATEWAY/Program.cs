
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using WEB.GATEWAY;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Middleware.GatewayRouteHandlerMiddleware;
using WEB.GATEWAY.Middleware.ReverseProxyMiddleware;
using WEB.GATEWAY.Models;
using WEB.GATEWAY.Services;
using WEB.UTILITY.Caching;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security;
using WEB.UTILITY.Security.ISecurity;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

// Load configuration and create builder
var builder = WebApplication.CreateBuilder(args);
// Read allowed origins from config
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
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

builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection(Constants.GATEWAY));

builder.Services.Configure<ReverseProxy>(builder.Configuration.GetSection(Constants.REVERSE_PROXY));

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection(Constants.REVERSE_PROXY))
.AddTransforms(builderContext =>
{
    builderContext.AddRequestTransform(async transformContext =>
    {
        await GatewayRoutingPathFilterByQueryParam(transformContext);
        GatewayRoutingTransformContext(transformContext);
        var incomingHeaders = transformContext.HttpContext.Request.Headers;

        var restrictedHeaders = new HashSet<string>(new[] { "Host", "Content-Length", "Transfer-Encoding" }, StringComparer.OrdinalIgnoreCase);
        // Copy headers from incoming request to proxy request, excluding restricted headers
        foreach (var header in incomingHeaders)
        {
            if (!restrictedHeaders.Contains(header.Key) &&
                !header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        // Safely handle Authorization header
        if (incomingHeaders.TryGetValue("Authorization", out var authHeader) &&
            !StringValues.IsNullOrEmpty(authHeader))
        {
            transformContext.ProxyRequest.Headers.Remove("Authorization");
            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("Authorization", authHeader.ToArray());
        }

        // Optionally: log or inspect headers for debugging
        foreach (var h in transformContext.ProxyRequest.Headers)
        {
            Log.ForContext<Program>().Debug($"{h.Key}: {string.Join(", ", h.Value)}");
        }
    });
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1")); // or your gateway IP
});

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

builder.Services.AddMemoryCache();
switch (builder.Configuration.GetSection("TurnOffRedisCache").Get<bool>())
{
    case false:
        {
            var redis = builder.Configuration.GetSection(Constants.REDIS);
            var redisCfg = redis.Get<RedisCfg>() ?? new RedisCfg();
            builder.Services.Configure<RedisCfg>(redis);
            builder.Services.AddStackExchangeRedisExtensions<StackExchange.Redis.Extensions.System.Text.Json.SystemTextJsonSerializer>(new RedisConfiguration
            {
                Hosts = [
                    new RedisHost
                    {
                        Host = redisCfg.Host,
                        Port = redisCfg.Port
                    }
                ],
                Ssl = redisCfg.UseSsl,
                User = redisCfg.Username,
                Password = redisCfg.Password,
                KeyPrefix = redisCfg.KeyPrefix,
                SyncTimeout = redisCfg.SyncTimeout
            });
            builder.Services.AddScoped<ICache>(s => new SafeCache(
                new RedisCache(
                    s.GetService<IRedisDatabase>()!,
                    s.GetService<IRedisClient>()!,
                    s.GetService<ILogger<RedisCache>>()!),
                s.GetService<ILogger<SafeCache>>()!));

            Log.ForContext<Program>().Information("Configuring Output Cache Redis Server Configuration");
        }
        break;
    default:
        {
            builder.Services.AddScoped<IMemoryCache, MemoryCache>();
            builder.Services.AddScoped<ICache>(s => new SafeCache(
                new InMemoryCache(s.GetService<IMemoryCache>()!, s.GetService<ILogger<InMemoryCache>>()!)
                , s.GetService<ILogger<SafeCache>>()!
                ));

            Log.ForContext<Program>().Information("Configuring Output Cache InMemory Configuration");
        }
        break;
}


builder.Services.Configure<RateLimitingConfig>(builder.Configuration.GetSection("RateLimiting"));

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

builder.Services.AddOutputCache(options =>
{
    var outputCache = builder.Configuration.GetSection("OutputCache").Get<OutputCacheOptions>();

    if (outputCache?.Policies != null)
    {
        Log.ForContext<Program>().Information("Configuring Output Cache policies");
        foreach (var (policyName, policy) in outputCache.Policies)
        {
            options.AddPolicy(policyName, policyBuilder =>
            {
                policyBuilder.Expire(policy.Duration);
            });
        }
    }
});

builder.Services.AddSingleton<IRateLimitConfigServiceProvider, RateLimitConfigServiceProvider>();
builder.Services.AddSingleton<IProxyConfigService, ProxyConfigServiceProvider>();
builder.Services.AddSingleton<IRoutingStrategy, DefaultRoutingStrategy>();
builder.Services.AddSingleton<IRateLimitingStrategy, PolicyBasedRateLimitingStrategy>();
builder.Services.AddSingleton<IForwardingStrategy, YarpForwardingStrategy>();
builder.Services.AddSingleton<IErrorHandlingStrategy, DefaultErrorHandlingStrategy>();

var app = builder.Build();
app.UseCors("AllowSpecificOrigins");

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseOutputCache();

Log.ForContext<Program>().Information("Gateway Ready");

app.MapReverseProxy(UseProxyPipeline());

await app.RunAsync();
await Log.CloseAndFlushAsync();
await app.DisposeAsync();
return;

Action<IReverseProxyApplicationBuilder> UseProxyPipeline()
{
    return proxy =>
    {
        proxy.UseMiddleware<ReverseProxyMiddleware>();
    };
}

static Task GatewayRoutingPathFilterByQueryParam(RequestTransformContext transformContext)
{
    GatewaySettings settings = new();
    if(transformContext is null || settings is null)
    {
        return Task.CompletedTask;
    }
    if (transformContext.HttpContext.Request.Path.HasValue && transformContext.HttpContext.Request.Path.Value.Equals(settings?.ApiPath, StringComparison.OrdinalIgnoreCase) && transformContext.HttpContext.Request.Query.ContainsKey(settings?.QueryKey!))
    {
        transformContext.Path = settings?.RoutePathByKey;

        var updatedParams = new Dictionary<string, StringValues>();

        foreach (var param in transformContext.HttpContext.Request.Query ?? Enumerable.Empty<KeyValuePair<string, StringValues>>())
        {
            var key = param.Key == settings?.QueryKey ? settings?.TransformQueryKey : param.Key;
            updatedParams[key!] = param.Value!;
        }
        transformContext.HttpContext.Request.Query = new QueryCollection(updatedParams);
    }
    return Task.CompletedTask;
}
