using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using WEB.SERVICES.IService;

namespace WEB.UTILITY.middleware
{
    public class TokenRevocationMiddleware
    {
        private readonly ITokenLifecycleService _tokenLifecycleService;
        private readonly RequestDelegate _next;

        public TokenRevocationMiddleware(RequestDelegate next, ITokenLifecycleService tokenLifecycleService)
        {
            _next = next;
            _tokenLifecycleService = tokenLifecycleService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var jti = _tokenLifecycleService.GetJtiFromToken(token);

                if (!string.IsNullOrEmpty(jti) && await _tokenLifecycleService.IsAccessTokenRevokedAsync(jti))
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
