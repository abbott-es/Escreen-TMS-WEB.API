namespace WEB.SERVICES.DTO.Generic
{
    public class ClientDto
    {
        public string? ClientID { get; set; }
        public string CompanyName { get; set; }

        public UserDto? User { get; set; }
        public IList<LocationDto>? Locations { get; set; }
    }
}
