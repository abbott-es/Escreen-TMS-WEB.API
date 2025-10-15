using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class TruckVendor : IEntity
    {
        public Guid TruckVendorID { get; set; }
        public Guid ID => TruckVendorID;
        public Guid UserID { get; set; }
        public string CompanyName { get; set; }
        public User User { get; set; }
        public ICollection<TruckHead> SuppliedTrucks { get; set; }
    }

}
