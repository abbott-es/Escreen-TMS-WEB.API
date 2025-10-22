using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Authentication
{
    public class Auth : IEntity
    {
        public Guid AuthID { get; set; }
        public Guid ID => AuthID;
        public Guid UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }
        public User User { get; set; }
    }
}
