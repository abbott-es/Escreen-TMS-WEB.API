using Microsoft.AspNetCore.Mvc.Filters;


namespace WEB.UTILITY.Caching
{

    /// <summary>
    /// An attribute that caches the response of a controller action using a static cache key.
    /// Use this when the response is the same for all users and requests.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheResponseAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _key;
        private readonly int _timeToLiveInSeconds;


        /// <summary>
        /// Initializes a new instance of the <see cref="CacheResponseAttribute"/> class.
        /// </summary>
        /// <param name="key">The static cache key to use.</param>
        /// <param name="timeToLiveInSeconds">The duration in seconds to keep the response in cache.</param>

        public CacheResponseAttribute(string key, int timeToLiveInSeconds)
        {
            _key = key;
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }


        /// <summary>
        /// Executes the caching logic before and after the action method runs.
        /// </summary>
        /// <param name="context">The context for the action executing.</param>
        /// <param name="next">The delegate to execute the next action filter or action method.</param>

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await CachingCommon.ExecuteAction(context, next, _key, _timeToLiveInSeconds);
        }
    }
}
