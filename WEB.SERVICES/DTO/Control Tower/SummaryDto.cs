
namespace WEB.SERVICES.DTO.Control_Tower
{
    public class SummaryDto
    {
        public Guid DriverUserID { get; set; }
        public Guid? HelperUserID { get; set; }
    }

    public class SummaryDetailDto : SummaryDto
    {
        public string DriverFullName { get; set; }
        public string? HelperFullName { get; set; }
    }
}
