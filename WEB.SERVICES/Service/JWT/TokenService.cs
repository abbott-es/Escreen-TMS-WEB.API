using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security;

namespace WEB.SERVICES.Service.JWT
{
    public class TokenService : BaseService<TokenService>, ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(JwtSettings settings, IAppLogger<TokenService> appLogger) : base(appLogger)
        {
            _settings = settings;
        }

        public (string accessToken, string jti) GenerateAccessToken(User user)
        {
            try
            {
                string jti = Guid.NewGuid().ToString();
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                    new Claim(ClaimTypes.Name, user.Auth.Username),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                DateTime expiresIn = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes);

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: expiresIn,
                    signingCredentials: creds);

                return (new JwtSecurityTokenHandler().WriteToken(token), jti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Not able to generate access token");
                return (string.Empty, string.Empty);
            }
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public Try<ClaimsPrincipal> ValidateAccessToken(string accessToken)
        {
            try
            {
                var validationParams = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                    ValidAlgorithms = new string[] { SecurityAlgorithms.HmacSha256 },
                    ValidateLifetime = false,
                    ValidAudience = _settings.Audience,
                    ValidIssuer = _settings.Issuer,
                };

                return () =>
                {
                    var principal = new JwtSecurityTokenHandler().ValidateToken(
                        accessToken,
                        validationParams,
                        out var securityToken);

                    return principal;
                };
            }
            catch (Exception err)
            {
                _logger.LogError(err, "Invalid access token");
                return null;
            }
        }
    }
}
