namespace WEB.SERVICES.DTO.Generic
{
    public class ChassisDto
    {
        public Guid ChassisID { get; set; }
        public Guid VehicleID { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }
    }
}
