
namespace WEB.SERVICES.DTO
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid RoleID { get; set; }
    }
}
