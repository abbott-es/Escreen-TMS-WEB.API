
namespace WEB.UTILITY.Security
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "WEB.AUTHENTICATION";
        public string Audience { get; set; } = "WEB.API";
        public string SecretKey { get; set; } = "your-super-secret-key";
        public int AccessTokenExpiryMinutes { get; set; } = 15;
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}
