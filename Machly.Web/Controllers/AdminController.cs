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
            var stats = await _adminApiClient.GetDashboardStatsAsync();
            return View(stats);
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

        // NOTIFICATIONS
        public IActionResult NotificationsCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotificationsCreate([FromServices] NotificationsApiClient notificationsApi, NotificationRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await notificationsApi.SendNotificationAsync(model);
            if (success)
            {
                TempData["Success"] = "Notificación enviada correctamente.";
                return RedirectToAction(nameof(NotificationsCreate));
            }

            ModelState.AddModelError("", "Error al enviar la notificación.");
            return View(model);
        }
        [HttpGet]
        public IActionResult MachineMap()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMachinesGeo()
        {
            var machines = await _adminApiClient.GetMachinesAsync(null, null, null);
            return Ok(machines);
        }
        [HttpGet]
        public async Task<IActionResult> ExportUsers()
        {
            // Proxy a la API
            // Asumimos que _adminApiClient tiene el HttpClient configurado
            // Pero AdminApiClient usa métodos tipados.
            // Podemos usar un HttpClient nuevo o inyectar IHttpClientFactory
            // Para simplificar, usaremos el mismo patrón que ChatController si es necesario, 
            // o mejor, extender AdminApiClient.
            // Vamos a extender AdminApiClient.
            var fileBytes = await _adminApiClient.GetUsersCsvAsync();
            return File(fileBytes, "text/csv", "users.csv");
        }

        [HttpGet]
        public async Task<IActionResult> ExportMachines()
        {
            var fileBytes = await _adminApiClient.GetMachinesCsvAsync();
            return File(fileBytes, "text/csv", "machines.csv");
        }
        [HttpGet]
        public async Task<IActionResult> Logs(DateTime? from, DateTime? to, string? userId, string? action)
        {
            var logs = await _adminApiClient.GetLogsAsync(from, to, userId, action);
            return View(logs);
        }
        [HttpGet]
        public IActionResult MachineImport()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportMachines(IFormFile file)
        {
            if (file == null) return View("MachineImport");

            // Enviar a API
            // Necesitamos usar HttpClient con MultipartFormDataContent
            // AdminApiClient no tiene soporte para esto, lo hacemos manual aquí o extendemos.
            // Haremos manual para rápido.
            
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);

            // Asumimos que _adminApiClient tiene el HttpClient expuesto o creamos uno nuevo con el token
            // Como _adminApiClient es un servicio tipado, no expone el cliente fácilmente a menos que sea public.
            // Usaremos IHttpClientFactory inyectado en el método si es posible, o creamos uno.
            // Pero necesitamos el token.
            // Mejor extender AdminApiClient.
            
            var success = await _adminApiClient.ImportMachinesAsync(file);
            if (success)
            {
                TempData["Success"] = "Importación completada.";
            }
            else
            {
                TempData["Error"] = "Error en la importación.";
            }

            return View("MachineImport");
        }
    }
}

