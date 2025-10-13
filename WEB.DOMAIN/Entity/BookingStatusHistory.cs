using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class BookingStatusHistory : IEntity
    {
        public Guid StatusID { get; set; }
        public Guid ID => StatusID;
        public Guid BookingID { get; set; }
        public string Status { get; set; }
        public Guid ChangedByUser { get; set; }
        public DateTime ChangedDate { get; set; }
        public Booking Booking { get; set; }
        public User User { get; set; }
    }
}