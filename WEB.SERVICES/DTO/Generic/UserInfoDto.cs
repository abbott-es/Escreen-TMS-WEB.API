namespace WEB.SERVICES.DTO.Generic
{
    public class UserInfoDto : BaseInfoDto
    {
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string LastName { get; set; }
    }

    public class BaseInfoDto
    {
        public Guid? UserInfoID { get; set; }
        public Guid? UserID { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string? LicenseNumber { get; set; }
    }
}
