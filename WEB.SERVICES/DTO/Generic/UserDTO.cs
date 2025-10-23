using System.Text.Json.Serialization;
using WEB.SERVICES.DTO.Authentication;

namespace WEB.SERVICES.DTO.Generic
{
    public class UserDto : IBaseDto
    {
        public Guid UserID { get; set; }
        [JsonIgnore]
        public Guid ID => UserID;
        public Guid RoleID { get; set; }
        public AuthDto? Auth { get; set; }
        public UserInfoDto? UserInfo { get; set; }
    }
}
