namespace WEB.SERVICES.DTO
{
    public class VehicleDto
    {
        public Guid VehicleID { get; set; }
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public ChassisDto Chassis { get; set; }
    }
}
