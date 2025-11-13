using LanguageExt;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WEB.UTILITY.Caching;
using WEB.UTILITY.LanguageExt;
using WEB.UTILITY.Logger;

namespace WEB.GATEWAY.Middleware;

public class CacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICache _cache;
    private readonly IAppLogger<CacheMiddleware> _logger;
    private static readonly string[] CacheableMethods = ["GET", "POST", "PUT"];

    public CacheMiddleware(
        RequestDelegate next,
        ICache cache,
        IAppLogger<CacheMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var cacheable = CacheableMethods.Contains(context.Request.Method);

        if (cacheable)
        {
            string cacheKey = await UTILITY.Helper.CacheHelper.GetCacheKey(context);

            Option<string> cached = await _cache.Get<string>(cacheKey);

            if (cached.IsSome)
            {
                _logger.LogDebug($"Fetch from gateway cache key: {cacheKey}");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status200OK;

                using var data = JsonDocument.Parse(cached.Value());
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    value: data.RootElement,
                    options: new JsonSerializerOptions
                    {
                        WriteIndented = true
                    })
                , context.RequestAborted);
                return;
            }
        }
        await _next(context);
    }

