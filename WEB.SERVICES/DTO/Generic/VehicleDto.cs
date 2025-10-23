using System.Text.Json.Serialization;

namespace WEB.SERVICES.DTO.Generic
{
    public class VehicleDto : IBaseDto
    {
        public Guid VehicleID { get; set; }
        [JsonIgnore]
        public Guid ID => VehicleID;
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public ChassisDto Chassis { get; set; }
    }
}
