using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Chassis : IEntity
    {
        public Guid ChassisID { get; set; }
        public Guid ID => ChassisID;
        public string Type { get; set; }
        public string SerialNumber { get; set; }
        public Guid VehicleID { get; set; }
        public Vehicle Vehicle { get; set; }
    }

}
