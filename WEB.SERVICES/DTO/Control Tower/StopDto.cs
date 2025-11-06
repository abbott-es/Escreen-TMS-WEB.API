namespace WEB.SERVICES.DTO
{
    public class StopDto
    {
        public CoordinateDto Coordinate { get; set; }
    }

    public class AddStopDto
    {
        public Guid BookingID { get; set; }
        public List<StopDto> Coordinates { get; set; }
    }
}