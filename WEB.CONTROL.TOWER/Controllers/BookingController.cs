using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService.IControl_Tower;
using WEB.UTILITY.Helper;

namespace WEB.CONTROL.TOWER.Controllers
{
    [Route("api/v1/[controller]")]
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
        /// <returns>201 Created if successful, 422 Unprocessable entity if validation fails.</returns>
        [HttpPost("create-booking")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_bookingService.CreateBookingAsync(bookingDto, ct));
        }

        /// <summary>
        /// Creates a new stop route to an existing booking.
        /// </summary>
        /// <param name="addStopDto">The stop route data.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>201 Created if successful, 422 Unprocessable entity if validation fails.</returns>
        [HttpPost("add-stop")]
        public async Task<IActionResult> AddStop([FromBody] AddStopDto addStopDto, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_bookingService.AddStopBookingAsync(addStopDto, ct));
        }

        /// <summary>
        /// Complete to an existing booking.
        /// </summary>
        /// <param name="id">Booking id</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>201 Created if successful, 422 Unprocessable entity if validation fails.</returns>
        [HttpPatch("complete-booking")]
        public async Task<IActionResult> AddStop([FromBody] Guid bookingID, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_bookingService.CompleteBookingAsync(bookingID, ct));
        }

        [HttpGet("get-booking-detail")]
        public async Task<IActionResult> GetBookingDetail([FromQuery] Guid bookingID, CancellationToken ct = default)
        {
            return null;
        }

        [HttpGet("get-all-bookings")]
        public async Task<IActionResult> GetAllBooking(CancellationToken ct = default)
        {
            return null;
        }

        [HttpGet("get-all-bookings-history")]
        public async Task<IActionResult> GetAllBookingHistory(CancellationToken ct = default)
        {
            return null;
        }
    }
}
