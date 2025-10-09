using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy;
using YarpRateLimitedProxy.Models;
using YarpRateLimitedProxy.Services;
using YarpRateLimitedProxy.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.Configure<ProxyConfigData>(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.Configure<RateLimitingConfig>(builder.Configuration.GetSection("RateLimiting"));

builder.Services.AddSingleton<ProxyConfigServiceProvider>();
builder.Services.AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());
builder.Services.AddSingleton<IProxyConfigService>(sp => sp.GetRequiredService<ProxyConfigServiceProvider>());

builder.Services.AddSingleton<RateLimitServiceProvider>();
builder.Services.AddSingleton<IRateLimitService>(sp => sp.GetRequiredService<RateLimitServiceProvider>());

builder.Services.AddReverseProxy();

builder.Services.AddRateLimiter(options =>
{
    var rateConfig = builder.Configuration.GetSection("RateLimiting").Get<RateLimitingConfig>();
    foreach (var kvp in rateConfig.Policies)
    {
        var policy = kvp.Value;
        options.AddPolicy(kvp.Key, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Request.Path.ToString(),
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = policy.PermitLimit,
                    Window = TimeSpan.Parse(policy.Window),
                    QueueLimit = policy.QueueLimit,
                    QueueProcessingOrder = Enum.Parse<QueueProcessingOrder>(policy.QueueProcessingOrder)
                }));
    }
});

var app = builder.Build();

app.UseRateLimiter();
app.UseMiddleware<ProxyServiceRateLimiterMiddleware>();
app.MapReverseProxy();

app.Run();
