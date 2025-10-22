using WEB.DOMAIN.Entity.Authentication;
using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class User : BaseEntity, IEntity
    {
        public Guid UserID { get; set; }
        public Guid ID => UserID;
        public Guid RoleID { get; set; }
        public Role Role { get; set; }

        public Auth Auth { get; set; }
        public Client Client { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
