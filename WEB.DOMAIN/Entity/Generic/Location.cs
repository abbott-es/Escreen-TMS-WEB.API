using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Location : IEntity
    {
        public Guid LocationID { get; set; }
        public Guid ID => LocationID;
        public string Name { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }

        public Guid? ClientID { get; set; }
        public Client? Client { get; set; }
    }
}
