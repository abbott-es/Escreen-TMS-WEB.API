
namespace WEB.SERVICES.DTO.Control_Tower
{
    public class SummaryDto
    {
        public Guid DriverUserID { get; set; }
        public Guid VehicleID { get; set; }
        public Guid? HelperUserID { get; set; }
    }

    public class SummaryDetailDto : SummaryDto
    {
        public string DriverFullName { get; set; }
        public string? HelperFullName { get; set; }
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public string Type { get; set; }
        public string SerialNumber { get; set; }
    }
}
