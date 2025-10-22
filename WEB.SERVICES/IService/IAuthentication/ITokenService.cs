using LanguageExt;
using System.Security.Claims;
using WEB.DOMAIN.Entity.Generic;

namespace WEB.SERVICES.IService.IAuthentication
{
    public interface ITokenService
    {
        (string accessToken, string jti) GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Try<ClaimsPrincipal> ValidateAccessToken(string accessToken, bool isValidateLifetime);
    }
}
