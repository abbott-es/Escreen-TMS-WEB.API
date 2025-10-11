namespace WEB.DOMAIN.Entity
{
    public class BookingStatusHistory
    {
        public Guid StatusID { get; set; }
        public Guid BookingID { get; set; }
        public string Status { get; set; }
        public Guid ChangedByUser { get; set; }
        public DateTime ChangedDate { get; set; }
        public Booking Booking { get; set; }
        public User User { get; set; }
    }
}