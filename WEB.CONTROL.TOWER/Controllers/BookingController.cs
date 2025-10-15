using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;

namespace WEB.CONTROL.TOWER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Creates a new booking.
        /// </summary>
        /// <param name="bookingDto">The booking data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>201 Created if successful, 400 Bad Request if validation fails.</returns>
        [HttpPost("create-booking")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto bookingDto, CancellationToken ct = default)
        {
            return null;
        }

        [HttpPost("add-stop")]
        public async Task<IActionResult> AddStop([FromBody] BookingDto bookingDto, CancellationToken ct = default)
        {
            return null;
        }

        [HttpGet("get-booking-detail")]
        public async Task<IActionResult> GetBookingDetail([FromBody] BookingDto bookingDto, CancellationToken ct = default)
        {
            return null;
        }
        [HttpGet("get-all-bookings")]
        public async Task<IActionResult> GetAllBooking([FromBody] BookingDto bookingDto, CancellationToken ct = default)
        {
            return null;
        }
        [HttpGet("get-all-bookings-history")]
        public async Task<IActionResult> GetAllBookingHistory([FromBody] BookingDto bookingDto, CancellationToken ct = default)
        {
            return null;
        }
    }
}
