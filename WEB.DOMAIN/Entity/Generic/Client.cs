using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Client : IEntity
    {
        public Guid ClientID { get; set; }
        public Guid ID => ClientID;
        public Guid UserID { get; set; }
        public string CompanyName { get; set; }

        public User User { get; set; }
        public ICollection<Location> Locations { get; set; }
    }

}
