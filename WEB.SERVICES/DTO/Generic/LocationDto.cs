using System.Text.Json.Serialization;

namespace WEB.SERVICES.DTO.Generic
{
    public class LocationDto : IBaseDto
    {
        public Guid LocationID { get; set; }
        [JsonIgnore]
        public Guid ID => LocationID;
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public Guid? ClientID { get; set; }
    }
}
