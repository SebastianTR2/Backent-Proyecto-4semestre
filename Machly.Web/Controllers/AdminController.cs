using Machly.Web.Models;
using Machly.Web.Models.Admin;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly AdminApiClient _adminApiClient;

        public AdminController(AdminApiClient adminApiClient)
        {
            _adminApiClient = adminApiClient;
        }

        public async Task<IActionResult> Dashboard()
        {
            var reports = await _adminApiClient.GetBasicReportsAsync();
            return View(reports);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _adminApiClient.GetUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var details = await _adminApiClient.GetUserDetailsAsync(id);
            if (details == null)
                return NotFound();

            return View(details);
        }

        public async Task<IActionResult> Machines(string? type, string? category, string? providerId)
        {
            var machines = await _adminApiClient.GetMachinesAsync(type, category, providerId);
            ViewData["TypeFilter"] = type;
            ViewData["CategoryFilter"] = category;
            ViewData["ProviderFilter"] = providerId;
            return View(machines);
        }

        public async Task<IActionResult> MachineDetails(string id)
        {
            var details = await _adminApiClient.GetMachineDetailsAsync(id);
            if (details == null)
                return NotFound();
            return View(details);
        }

        public async Task<IActionResult> Bookings(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            var bookings = await _adminApiClient.GetBookingsAsync(start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyProvider(string userId, bool isVerified, string? returnUrl = null)
        {
            var result = await _adminApiClient.VerifyProviderAsync(userId, isVerified);
            if (result)
                return string.IsNullOrEmpty(returnUrl)
                    ? RedirectToAction(nameof(Users))
                    : Redirect(returnUrl);

            return BadRequest();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMachine(string id)
        {
            var deleted = await _adminApiClient.DeleteMachineAsync(id);
            if (deleted)
                return RedirectToAction(nameof(Machines));

            return BadRequest();
        }
    }
}

