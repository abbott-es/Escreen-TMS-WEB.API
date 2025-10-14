using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Caching
{

    /// <summary>
    /// An attribute that caches the response of a controller action using a dynamically generated cache key.
    /// The key is based on request path, query parameters, and optionally user-specific data.
    /// </summary>
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _keyPrefix;
        private readonly int _timeToLiveInSeconds;


        /// <summary>
        /// Initializes a new instance of the <see cref="CachedAttribute"/> class with no key prefix.
        /// </summary>
        /// <param name="timeToLiveInSeconds">The duration in seconds to keep the response in cache.</param>

        public CachedAttribute(int timeToLiveInSeconds)
        {
            _keyPrefix = string.Empty;
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachedAttribute"/> class with a custom key prefix.
        /// </summary>
        /// <param name="keyPrefix">A prefix to prepend to the generated cache key.</param>
        /// <param name="timeToLiveInSeconds">The duration in seconds to keep the response in cache.</param>

        private protected CachedAttribute(string keyPrefix, int timeToLiveInSeconds)
        {
            _keyPrefix = keyPrefix;
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }

        /// <summary>
        /// Executes the caching logic before and after the action method runs.
        /// </summary>
        /// <param name="context">The context for the action executing.</param>
        /// <param name="next">The delegate to execute the next action filter or action method.</param>

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var key = GenerateCacheKey(context.HttpContext);
            await CachingCommon.ExecuteAction(context, next, key, _timeToLiveInSeconds);
        }


        /// <summary>
        /// Generates a unique cache key based on the request path, query parameters, and optionally user info.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <returns>A string representing the cache key.</returns>

        private string GenerateCacheKey(HttpContext context)
        {
            var builder = new StringBuilder();
            builder.Append(_keyPrefix);
            if (context.Request.Headers[HeaderNames.Authorization].FirstOrDefault() != null)
            {
                //use (_, string, string) = context.User...
                //then builder.append(...)
            }

            builder.Append(context.Request.Path);

            foreach (var kvp in context.Request.Query.OrderBy(x => x.Key))
                builder.Append($"-{kvp.Key}-{kvp.Value}");

            return builder.ToString();
        }
    }
}
