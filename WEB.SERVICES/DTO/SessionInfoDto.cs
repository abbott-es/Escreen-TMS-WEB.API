
namespace WEB.SERVICES.DTO
{
    public class SessionInfoDto
    {
        public Guid TokenId { get; set; }
        public string RefreshToken { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenJti { get; set; }
        public DateTime ExpiryIn { get; set; }
    }
}
