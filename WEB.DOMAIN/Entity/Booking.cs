using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Booking : BaseEntity, IEntity
    {
        public Guid BookingID { get; set; }
        public Guid ID => BookingID;
        public Guid? ClientID { get; set; }
        public Guid? LocationID { get; set; }
        public Guid? TruckID { get; set; }
        public Guid? DriverID { get; set; }
        public Guid? HelperID { get; set; }
        public Guid? StartRouteID { get; set; }
        public Guid? EndRouteID { get; set; }
        public Guid? StopRouteID { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string Status { get; set; }
        // Navigation properties
        public Client Client { get; set; }
        public Location Location { get; set; }
        public TruckHead TruckHead { get; set; }
        public Driver Driver { get; set; }
        public Helper Helper { get; set; }
        public ICollection<BookingStatusHistory> StatusHistory { get; set; }

        public Coordinate StartRoute { get; set; }
        public Coordinate EndRoute { get; set; }
        public ICollection<Stop> StopRoute { get; set; }
    }

}
