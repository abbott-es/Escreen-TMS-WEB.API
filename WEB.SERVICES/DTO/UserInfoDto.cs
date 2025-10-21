
namespace WEB.SERVICES.DTO
{
    public class UserInfoDto
    {
        public Guid? UserInfoID { get; set; }
        public Guid? UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
