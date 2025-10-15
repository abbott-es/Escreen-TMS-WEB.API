using WEB.DOMAIN.Interface;

namespace WEB.DOMAIN.Entity
{
    public class Stop : IEntity
    {
        public Guid StopID { get; set; }
        public Guid ID => StopID;
        public Guid CoordinateID { get; set; }
        public Coordinate Coordinate { get; set; }

        public Guid BookingID { get; set; }
        public Booking Booking { get; set; }
    }
}
