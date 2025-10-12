using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class User : BaseEntity, IEntity
    {
        public Guid UserID { get; set; }
        public Guid ID => UserID;
        public Guid RoleID { get; set; }
        public Role Role { get; set; }

        public Auth Auth { get; set; }
        public Driver Driver { get; set; }
        public Client Client { get; set; }
        public Helper Helper { get; set; }
        public TruckVendor TruckVendor { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
