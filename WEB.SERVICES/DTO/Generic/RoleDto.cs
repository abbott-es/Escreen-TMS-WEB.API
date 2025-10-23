using System.Text.Json.Serialization;

namespace WEB.SERVICES.DTO.Generic
{
    public class RoleDto : IBaseDto
    {
        public Guid? RoleId { get; set; }
        [JsonIgnore]
        public Guid ID => RoleId.Value;
        public string RoleName { get; set; }
    }
}
