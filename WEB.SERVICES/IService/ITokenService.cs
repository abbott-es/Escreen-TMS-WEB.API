using LanguageExt;
using System.Security.Claims;
using WEB.DOMAIN.Entity;

namespace WEB.SERVICES.IService
{
    public interface ITokenService
    {
        (string accessToken, string jti) GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Try<ClaimsPrincipal> ValidateAccessToken(string accessToken, bool isValidateLifetime);
    }
}
