using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("history")]
    [Authorize]
    public class HistoryController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public HistoryController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET /history/user/{id} - Historial por usuario (solo el mismo usuario o ADMIN)
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserHistory(string id)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (userId != id && role != "ADMIN")
                return Forbid();

            var bookings = await _bookingService.GetByUserAsync(id);
            return Ok(bookings);
        }

        // GET /history/machine/{id} - Historial por máquina
        [HttpGet("machine/{id}")]
        public async Task<IActionResult> GetMachineHistory(string id)
        {
            var bookings = await _bookingService.GetByMachineAsync(id);
            return Ok(bookings);
        }
    }
}

