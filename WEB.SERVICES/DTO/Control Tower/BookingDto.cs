namespace WEB.SERVICES.DTO
{
    public class BookingDto
    {
        public CoordinateDto StartRoute { get; set; }
        public CoordinateDto EndRoute { get; set; }
        public List<StopDto>? StopRoute { get; set; }

    }
}
