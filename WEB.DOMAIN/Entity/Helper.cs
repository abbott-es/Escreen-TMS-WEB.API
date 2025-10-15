using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Helper : IEntity
    {
        public Guid HelperID { get; set; }
        public Guid ID => HelperID;
        public Guid UserID { get; set; }
        public Guid AssignedDriverID { get; set; }

        public User User { get; set; }
        public Driver AssignedDriver { get; set; }
    }

}
