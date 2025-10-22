
namespace WEB.SERVICES.DTO
{
    public class LocationDto
    {
        public Guid LocationID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public Guid? ClientID { get; set; }
    }
}
