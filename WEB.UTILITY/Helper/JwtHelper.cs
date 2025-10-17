using System.IdentityModel.Tokens.Jwt;

namespace WEB.UTILITY.Helper
{
    public static class JwtHelper
    {
        public static bool TryExtractJti(string accessToken, out string? jti)
        {
            jti = null;

            if (string.IsNullOrWhiteSpace(accessToken))
                return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(accessToken))
                    return false;

                var jwtToken = handler.ReadJwtToken(accessToken);

                jti = jwtToken?.Id;
                return !string.IsNullOrEmpty(jti);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
