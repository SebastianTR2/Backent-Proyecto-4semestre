using System;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("renter")]
    [Authorize(Roles = "RENTER")]
    public class RenterController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public RenterController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET /renter/bookings - Historial del renter logueado
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var renterId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(renterId))
                return Unauthorized();

            var bookings = await _bookingService.GetByUserAsync(renterId, from, to);
            return Ok(bookings);
        }
    }
}

