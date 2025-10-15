using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.middleware
{
    public class JwtThrottlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly int _limit = 20;
        private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

        public JwtThrottlingMiddleware(RequestDelegate next, IMemoryCache cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = GetThrottleKey(context);
            var counter = _cache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _window;
                return new RequestCounter { Count = 0 };
            });

            counter.Count++;

            int remaining = Math.Max(0, _limit - counter.Count);

            context.Response.Headers["X-RateLimit-Limit"] = _limit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();
            context.Response.Headers["X-RateLimit-Reset"] = DateTimeOffset.UtcNow.Add(_window).ToUnixTimeSeconds().ToString();

            if (counter.Count > _limit)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = _window.TotalSeconds.ToString();
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            await _next(context);
        }

        private string GetThrottleKey(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var path = context.Request.Path.ToString().ToLower();

            return userId != null
                ? $"throttle:user:{userId}:{path}"
                : $"throttle:ip:{ip}:{path}";
        }

        private class RequestCounter
        {
            public int Count { get; set; }
        }
    }

}
