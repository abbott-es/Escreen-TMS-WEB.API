namespace WEB.SERVICES.DTO
{
    public class StopDto
    {
        public CoordinateDto Coordinates { get; set; }
    }

    public class AddStopDto : StopDto
    {
        public Guid BookingID { get; set; }
    }
}