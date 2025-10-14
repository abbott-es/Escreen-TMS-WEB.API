using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WEB.UTILITY.LanguageExt;
using WEB.UTILITY.Serialization;

namespace WEB.UTILITY.Caching
{
    public static class CachingCommon
    {
        public static async Task ExecuteAction(ActionExecutingContext context, ActionExecutionDelegate next,
        string key, int timeToLive)
        {
            if (context.HttpContext.Request.Method != "GET")
            {
                await next();
                return;
            }

            var cache = context.HttpContext.RequestServices.GetRequiredService<ICache>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<CachedAttribute>>();
            var cachedResponse = await cache.Get<string>(key);

            if (cachedResponse.IsSome)
            {
                logger.LogInformation("Returning response from cache. Cache key: {cacheKey}.", key);
                var contentResult = new ContentResult
                {
                    Content = cachedResponse.Value(),
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }

            var executedContext = await next();
            if (executedContext.Result is OkObjectResult okObjectResult && okObjectResult.Value != null)
            {
                logger.LogInformation("Saving response to cache. Cache key: {cacheKey}.", key);
                await cache.Set<string>(
                    key,
                    JsonSerializer.Serialize(okObjectResult.Value, SerializationBuilder.Options()),
                    TimeSpan.FromSeconds(timeToLive));
            }
        }
    }
}
