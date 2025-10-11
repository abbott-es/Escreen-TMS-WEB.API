using System;
using System.IO;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Web.Gateway.Middleware;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.GATEWAY.Services;
using WEB.UTILITY.Logger;

// Load configuration and create builder
var builder = WebApplication.CreateBuilder(args);
// Load configuration files
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
// Load environment-specific configuration if it exists
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
// Setup Logger
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext().CreateLogger();

builder.Host.UseSerilog();
Log.ForContext<Program>().Information(
    "API Gateway Initialized in {Environment} environment {Date} on {Urls}",
    builder.Environment.EnvironmentName,
    DateTime.UtcNow.ToUniversalTime(),
    builder.Configuration.GetValue<string>("ASPNETCORE_URLS") ?? "not set"
);

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

// Setup Reverse Proxy
Log.ForContext<Program>().Information("Setting up Reverse Proxy");
builder.Services.Configure<ReverseProxy>(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddSingleton<ProxyConfigServiceProvider>();
builder.Services.AddSingleton<Yarp.ReverseProxy.Configuration.IProxyConfigProvider>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());
builder.Services.AddSingleton<IProxyConfigService>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());
builder.Services.AddReverseProxy();

// Setup Rate Limiting
builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimiting"));
builder.Services.AddSingleton<IRateLimitConfigServiceProvider, RateLimitConfigServiceProvider>();

// Register Rate Limiter Policies
builder.Services.AddRateLimiter(options =>
{
    var rateLimitConfig = builder.Configuration.Get<RateLimitConfig>();
    if (rateLimitConfig?.Policies != null)
    {
        Log.ForContext<Program>().Information("Configuring Rate Limiting policies");
        foreach (var (policyName, policy) in rateLimitConfig.Policies)
        {
            options.AddPolicy(policyName, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Request.Path.ToString(),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = policy.PermitLimit,
                        Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                        QueueLimit = policy.QueueLimit,
                        QueueProcessingOrder = Enum.Parse<QueueProcessingOrder>(policy.QueueProcessingOrder, ignoreCase: true)
                    }));
        }
    }
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        return RateLimitPartition.GetNoLimiter("NoRateLimitingPolicy");
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too Many Requests. Please try again later.", cancellationToken: token);
    };
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseMiddleware<ReverseProxyServiceMiddleware>();
app.MapReverseProxy();

await app.RunAsync();
await Log.CloseAndFlushAsync();