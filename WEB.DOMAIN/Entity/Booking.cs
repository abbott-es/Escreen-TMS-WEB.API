using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Booking : BaseEntity, IEntity
    {
        public Guid BookingID { get; set; }
        public Guid ID => BookingID;
        public Guid ClientID { get; set; }
        public Guid LocationID { get; set; }
        public Guid VehicleID { get; set; }
        public Guid DriverUserID { get; set; }
        public Guid? HelperUserID { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; }
        // Navigation properties
        public Client Client { get; set; }
        public Location Location { get; set; }
        public Vehicle Vehicle { get; set; }
        public User Driver { get; set; }
        public User Helper { get; set; }
        public ICollection<BookingStatusHistory> StatusHistory { get; set; }
    }

}
