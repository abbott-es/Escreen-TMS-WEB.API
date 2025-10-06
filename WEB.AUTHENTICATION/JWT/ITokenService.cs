using System.Security.Claims;

namespace WEB.AUTHENTICATION.JWT
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
    }
}
