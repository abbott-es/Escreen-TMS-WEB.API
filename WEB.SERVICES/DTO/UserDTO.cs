
namespace WEB.SERVICES.DTO
{
    public class UserDto
    {
        public Guid RoleID { get; set; }
        public UserInfoDto UserInfo { get; set; }
        public AuthDto Auth { get; set; }
    }
}
