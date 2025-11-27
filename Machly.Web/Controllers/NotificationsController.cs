using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Machly.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly NotificationsApiClient _apiClient;

        public NotificationsController(NotificationsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var notifications = await _apiClient.GetByUserAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            await _apiClient.MarkAsReadAsync(id);
            return RedirectToAction("Index");
        }
    }
}
