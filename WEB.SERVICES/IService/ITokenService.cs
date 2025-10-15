using System.Security.Claims;
using WEB.DOMAIN.Entity;

namespace WEB.SERVICES.IService
{
    public interface ITokenService
    {
        (string accessToken, DateTime expiresIn) GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
