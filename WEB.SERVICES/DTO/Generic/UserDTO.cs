using WEB.SERVICES.DTO.Authentication;

namespace WEB.SERVICES.DTO.Generic
{
    public class UserDto
    {
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public AuthDto? Auth { get; set; }
        public UserInfoDto? UserInfo { get; set; }
    }
}
