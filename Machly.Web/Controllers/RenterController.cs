using Machly.Web.Models;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Machly.Web.Controllers
{
    [Authorize(Policy = "RenterOnly")]
    public class RenterController : Controller
    {
        private readonly BookingsApiClient _bookingsApiClient;
        private readonly MachinesApiClient _machinesApiClient;

        public RenterController(
            BookingsApiClient bookingsApiClient,
            MachinesApiClient machinesApiClient)
        {
            _bookingsApiClient = bookingsApiClient;
            _machinesApiClient = machinesApiClient;
        }

        public async Task<IActionResult> Index(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            var bookings = await _bookingsApiClient.GetByUserAsync(userId, start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(string bookingId, int rating, string comment)
        {
            var result = await _bookingsApiClient.AddReviewAsync(bookingId, rating, comment);
            if (result)
                return RedirectToAction(nameof(Index));

            return BadRequest();
        }
    }
}

