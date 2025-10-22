using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity.Generic
{
    public class Vehicle : IEntity
    {
        public Guid VehicleID { get; set; }
        public Guid ID => VehicleID;
        public string Model { get; set; }
        public string PlateNumber { get; set; }
        public Chassis Chassis { get; set; }
    }

}
