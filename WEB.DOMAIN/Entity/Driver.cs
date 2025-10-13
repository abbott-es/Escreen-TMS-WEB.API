using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Driver : IEntity
    {
        public Guid DriverID { get; set; }
        public Guid ID => DriverID;
        public Guid UserID { get; set; }
        public string LicenseNumber { get; set; }
        public Guid? AssignedTruckID { get; set; }

        public User User { get; set; }

        public TruckHead AssignedTruck { get; set; }
        public ICollection<Helper> Helpers { get; set; }
    }

}
