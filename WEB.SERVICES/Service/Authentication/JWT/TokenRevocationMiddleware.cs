using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WEB.SERVICES.IService.IAuthentication;

namespace WEB.SERVICES.Service.Authentication.JWT
{
    public class TokenRevocationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public TokenRevocationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token) && !token.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var tokenLifecycleService = scope.ServiceProvider.GetRequiredService<ITokenLifecycleService>();
                var jti = tokenLifecycleService.GetJtiFromToken(token);

                if (!string.IsNullOrEmpty(jti) && await tokenLifecycleService.IsAccessTokenRevokedAsync(jti))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Access token has been revoked.");
                    return;
                }
            }

            await _next(context);
        }
    }
}