using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Role : BaseEntity, IEntity
    {
        public Guid RoleID { get; set; }
        public Guid ID => RoleID;
        public string RoleName { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
