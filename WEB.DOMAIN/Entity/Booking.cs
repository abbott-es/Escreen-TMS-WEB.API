using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Booking : BaseEntity, IEntity
    {
        public Guid BookingID { get; set; }
        public Guid ClientID { get; set; }
        public Guid LocationID { get; set; }
        public Guid TruckID { get; set; }
        public Guid DriverID { get; set; }
        public Guid? HelperID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; }
        // Navigation properties
        public Client Client { get; set; }
        public Location Location { get; set; }
        public TruckHead TruckHead { get; set; }
        public Driver Driver { get; set; }
        public Helper Helper { get; set; }
        public ICollection<BookingStatusHistory> StatusHistory { get; set; }
    }

}
