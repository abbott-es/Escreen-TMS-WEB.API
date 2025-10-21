using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class UserInfo : IEntity
    {
        public Guid UserInfoID { get; set; }
        public Guid ID => UserInfoID;
        public Guid UserID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string? LicenseNumber { get; set; }

        public User User { get; set; }
    }
}
