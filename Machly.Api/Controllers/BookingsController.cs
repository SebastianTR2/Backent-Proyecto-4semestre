using Machly.Api.DTOs;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("bookings")]
    public class BookingsController : ControllerBase
    {
        private readonly BookingService _service;

        public BookingsController(BookingService service)
        {
            _service = service;
        }

        // POST /bookings - Solo RENTER
        [HttpPost]
        [Authorize(Roles = "RENTER")]
        public async Task<IActionResult> Create([FromBody] BookingCreateRequest request)
        {
            // Obtener RenterId del JWT
            var renterId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(renterId))
                return Unauthorized();

            request.RenterId = renterId;

            var booking = await _service.CreateAsync(request);
            if (booking == null)
                return BadRequest("Machine not found or not available");

            return Ok(booking);
        }

        // GET /bookings/user/{renterId} - Solo el mismo usuario o ADMIN
        [HttpGet("user/{renterId}")]
        [Authorize]
        public async Task<IActionResult> GetByUser(string renterId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (userId != renterId && role != "ADMIN")
                return Forbid();

            var bookings = await _service.GetByUserAsync(renterId, from, to);
            return Ok(bookings);
        }

        // POST /bookings/{id}/checkin - Solo PROVIDER
        [HttpPost("{id}/checkin")]
        [Authorize(Roles = "PROVIDER")]
        public async Task<IActionResult> CheckIn(string id, [FromBody] CheckInRequest request)
        {
            var result = await _service.CheckInAsync(id, request.Photos);
            if (!result)
                return BadRequest("Booking not found or cannot check in");

            return Ok(new { message = "Check-in successful" });
        }

        // POST /bookings/{id}/checkout - Solo PROVIDER
        [HttpPost("{id}/checkout")]
        [Authorize(Roles = "PROVIDER")]
        public async Task<IActionResult> CheckOut(string id, [FromBody] CheckOutRequest request)
        {
            var result = await _service.CheckOutAsync(id, request.Photos);
            if (!result)
                return BadRequest("Booking not found or cannot check out");

            return Ok(new { message = "Check-out successful" });
        }

        // POST /bookings/review/{id} - Solo RENTER
        [HttpPost("review/{id}")]
        [Authorize(Roles = "RENTER")]
        public async Task<IActionResult> AddReview(string id, [FromBody] ReviewRequest request)
        {
            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var result = await _service.AddReviewAsync(id, request.Rating, request.Comment);
            if (!result)
                return BadRequest("Booking not found or cannot add review");

            return Ok(new { message = "Review added successfully" });
        }
    }
}
