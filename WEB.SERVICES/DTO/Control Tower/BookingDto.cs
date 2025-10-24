namespace WEB.SERVICES.DTO
{
    public class BookingDto
    {
        public Guid VehicleID { get; set; }
        public Guid DriverUserID { get; set; }
        public Guid? HelperUserID { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public CoordinateDto StartRoute { get; set; }
        public CoordinateDto EndRoute { get; set; }
        public List<StopDto>? StopRoute { get; set; }
    }

    public class CreateBookingDto : BookingDto
    {

    }
}
