using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Chassis : IEntity
    {
        public Guid ChassisID { get; set; }
        public Guid ID => ChassisID;
        public string Type { get; set; }
        public string SerialNumber { get; set; }
        public Guid TruckHeadID { get; set; }
        public TruckHead TruckHead { get; set; }
    }

}
