using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.RateLimiting;
using WEB.GATEWAY;
using WEB.GATEWAY.Interfaces;
using Middleware = WEB.GATEWAY.Middleware;
using WEB.GATEWAY.Models;
using WEB.GATEWAY.Services;
using WEB.GATEWAY.Transforms;
using WEB.UTILITY.Caching;
using WEB.UTILITY.Logger;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms.Builder;

// Load configuration and create builder
var builder = WebApplication.CreateBuilder(args);

// Load configuration files
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Load environment-specific configuration if it exists
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Setup Logger
builder.Host.UseSerilog((context, logConfig) => logConfig.ReadFrom.Configuration(context.Configuration).Enrich.FromLogContext().CreateLogger());

// Read allowed origins from config
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    options.AddPolicy("AllowSpecificOrigins", policy => policy.WithOrigins(allowedOrigins!).AllowAnyHeader().AllowAnyMethod());
});

Log.ForContext<Program>().Information(
    "API Gateway Initialized in {Environment} environment {Date} on {Urls}",
    builder.Environment.EnvironmentName,
    DateTime.UtcNow.ToUniversalTime(),
    builder.Configuration.GetValue<string>("ASPNETCORE_URLS") ?? "Unknown"
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

builder.Services.Configure<GatewaySettings>(builder.Configuration.GetSection(Constants.GATEWAY));
builder.Services.Configure<ReverseProxy>(builder.Configuration.GetSection(Constants.REVERSE_PROXY));
builder.Services.Configure<OutputCacheOptions>(builder.Configuration.GetSection(Constants.OUTPUT_CACHE));
builder.Services.Configure<RateLimitingConfig>(builder.Configuration.GetSection(Constants.RATE_LIMITNG));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownProxies.Add(IPAddress.Parse("127.0.0.1")); // or your gateway IP
});

builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));
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

builder.Services.AddSingleton<IOutputCacheService, OutputCacheService>();

builder.Services.AddSingleton<ITransformProvider, GatewayTransformProvider>();
builder.Services.AddSingleton<IRateLimitConfigServiceProvider, RateLimitConfigServiceProvider>();
builder.Services.AddSingleton<IProxyConfigService, ProxyConfigServiceProvider>();

builder.Services.AddSingleton<IRoutingStrategy, Middleware.DefaultRoutingStrategy>();
builder.Services.AddSingleton<IRateLimitingStrategy, Middleware.PolicyBasedRateLimitingStrategy>();
builder.Services.AddSingleton<IForwardingStrategy, Middleware.YarpForwardingStrategy>();
builder.Services.AddSingleton<IErrorHandlingStrategy, Middleware.DefaultErrorHandlingStrategy>();

// Setup Reverse Proxy
Log.ForContext<Program>().Information("Setting up Reverse Proxy");

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection(Constants.REVERSE_PROXY));

// Setup Cache Host
switch (builder.Configuration.GetSection(Constants.NO_REDIS).Get<bool>())
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
            new RedisCache(s.GetRequiredService<IRedisDatabase>(), s.GetRequiredService<IRedisClient>(), s.GetRequiredService<ILogger<RedisCache>>()),
            s.GetRequiredService<ILogger<SafeCache>>()));

        Log.ForContext<Program>().Information("Configuring Output Cache Redis Server Configuration");
    }
    break;
    default:
    {
        builder.Services.AddScoped<IMemoryCache, MemoryCache>();
        builder.Services.AddScoped<ICache>(s => new SafeCache(
            new InMemoryCache(s.GetRequiredService<IMemoryCache>(), s.GetRequiredService<ILogger<InMemoryCache>>())
            , s.GetRequiredService<ILogger<SafeCache>>()));

        Log.ForContext<Program>().Information("Configuring Output Cache InMemory Configuration");
    }
    break;
}

// Setup Rate RateLiming Policies
builder.Services.AddRateLimiter(options =>
{
    var rateLimitConfig = builder.Configuration.GetSection(Constants.RATE_LIMITNG).Get<RateLimitingConfig>();

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

// Setup Output cache Service
builder.Services.AddOutputCache(options =>
{
    var outputCache = builder.Configuration.GetSection(Constants.OUTPUT_CACHE).Get<OutputCacheOptions>();

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

var app = builder.Build();

app.UseCors("AllowSpecificOrigins");
app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseOutputCache();

Log.ForContext<Program>().Information("Gateway Ready");

app.UseMiddleware<Middleware.RateLimitMiddleware>();
app.UseMiddleware<Middleware.CacheMiddleware>();
app.UseMiddleware<Middleware.GatewayRouteHandlerMiddleware>();
app.MapReverseProxy(proxy => proxy.UseMiddleware<Middleware.ReverseProxyMiddleware>());

await app.RunAsync();
await Log.CloseAndFlushAsync();
await app.DisposeAsync();
return;
