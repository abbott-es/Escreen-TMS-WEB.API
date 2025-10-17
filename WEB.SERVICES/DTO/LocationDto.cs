
namespace WEB.SERVICES.DTO
{
    public class LocationDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public Guid? ClientID { get; set; }
    }
}
