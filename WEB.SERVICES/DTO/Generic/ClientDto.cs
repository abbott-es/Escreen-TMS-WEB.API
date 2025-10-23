using System.Text.Json.Serialization;

namespace WEB.SERVICES.DTO.Generic
{
    public class ClientDto : IBaseDto
    {
        public Guid? ClientID { get; set; }
        [JsonIgnore]
        public Guid ID => ClientID.Value;
        public string CompanyName { get; set; }

        public UserDto? User { get; set; }
        public IList<LocationDto>? Locations { get; set; }
    }
}
