
namespace WEB.SERVICES.DTO
{
    public class UserDto : UpdateUserDto
    {
        public AuthDto Auth { get; set; }
    }

    public class UpdateUserDto
    {
        public Guid? UserID { get; set; }
        public Guid RoleID { get; set; }
        public UserInfoDto UserInfo { get; set; }
    }
}
