using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Threading.RateLimiting;
using WEB.GATEWAY.Interfaces;
using WEB.GATEWAY.Models;
using WEB.GATEWAY.Services;

namespace WEB.GATEWAY;

public static class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.LoadDependencyPipelines();

        var app = builder.Build();

        app.UseRateLimiter();
        app.MapReverseProxy();

        // Add Custom Mapping
        app.MapGatewayApiRouting();
        app.MapGet("/", () => "Hello World!");

        app.Run();
        Log.CloseAndFlush();
    }

    /// <summary>
    /// Load the Gateway Route to Backend Apis
    /// </summary>
    /// <param name="app"></param>
    private static void MapGatewayApiRouting(this WebApplication app)
    {

    }

    /// <summary>
    /// Initialize the logging system setup
    /// </summary>
    /// <param name="builder"></param>
    private static void SetupLogger(this WebApplicationBuilder builder)
    {

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
        Log.Information("API Gateway Initialized");

    }

    /// <summary>
    /// Loads all dependency
    /// </summary>
    /// <param name="builder"></param>
    private static void LoadDependencyPipelines(this WebApplicationBuilder builder)
    {
        // Load configuration files
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.SetupLogger();

        // Setup Reverse Proxy
        builder.Services.Configure<ReverseProxy>(builder.Configuration.GetSection("ReverseProxy"));
        builder.Services.LoadGatewayProxyConfigService();

        builder.Services.Configure<RateLimitConfig>(builder.Configuration.GetSection("RateLimiting"));

        builder.Services.LoadGatewayProxyRateLimiter();
    }

    private static void LoadGatewayProxyConfigService(this IServiceCollection service)
    {

        service.AddSingleton<ProxyConfigServiceProvider>();
        service.AddSingleton<Yarp.ReverseProxy.Configuration.IProxyConfigProvider>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());
        service.AddSingleton<IProxyConfigService>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());

        IProxyConfigService pxyCfgSvc = service.BuildServiceProvider().GetRequiredService<IProxyConfigService>();

        service.AddReverseProxy().LoadFromMemory(routes: pxyCfgSvc.GetRoutes(), clusters: pxyCfgSvc.GetClusters());

    }

    private static void LoadGatewayProxyRateLimiter(this IServiceCollection service)
    {
        service.AddSingleton<IRateLimitConfigServiceProvider, RateLimitConfigServiceProvider>();

        var rateConfig = service.BuildServiceProvider().GetRequiredService<IRateLimitConfigServiceProvider>()
            .GetAllPolicies();

        service.AddRateLimiter(options =>
        {
            foreach (var (key, policy) in rateConfig)
            {
                options.AddPolicy(key, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Request.Path.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = policy.PermitLimit,
                            Window = TimeSpan.FromSeconds(policy.WindowSeconds),
                            QueueLimit = policy.QueueLimit,
                            QueueProcessingOrder = Enum.Parse<QueueProcessingOrder>(policy.QueueProcessingOrder)
                        }));
            }
        });

    }

}