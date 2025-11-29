using Machly.Web.Models;
using Machly.Web.Models.Admin;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly AdminApiClient _adminApiClient;
        private readonly SettingsApiClient _settingsApiClient;

        public AdminController(AdminApiClient adminApiClient, SettingsApiClient settingsApiClient)
        {
            _adminApiClient = adminApiClient;
            _settingsApiClient = settingsApiClient;
        }

        public async Task<IActionResult> Dashboard()
        {
            var stats = await _adminApiClient.GetDashboardStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> Users(string? role)
        {
            var users = await _adminApiClient.GetUsersAsync(role);
            return View(users);
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            var details = await _adminApiClient.GetUserDetailsAsync(id);
            if (details == null)
                return NotFound();

            return View(details);
        }

        [HttpGet]
        public IActionResult UserCreate()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserCreate(User user)
        {
            // Simple validation
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Role))
            {
                if (string.IsNullOrEmpty(user.Email))
                    ModelState.AddModelError("Email", "Email is required");
                if (string.IsNullOrEmpty(user.Role))
                    ModelState.AddModelError("Role", "Role is required");
                return View(user);
            }

            // Simple email format validation
            if (!user.Email.Contains("@") || !user.Email.Contains(".com"))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View(user);
            }

            // Password required for creation
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Password is required");
                return View(user);
            }

            var result = await _adminApiClient.GetCreateUserResultAsync(user);
            if (result.Success)
                return RedirectToAction(nameof(Users));

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                if (result.ErrorMessage.Contains("Email"))
                    ModelState.AddModelError("Email", result.ErrorMessage);
                else
                    ModelState.AddModelError("", result.ErrorMessage);
            }
            else
            {
                ModelState.AddModelError("", "Error creating user. Email might be taken.");
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> UserEdit(string id)
        {
            var details = await _adminApiClient.GetUserDetailsAsync(id);
            if (details == null || details.User == null)
                return NotFound();

            var user = new User
            {
                Id = details.User.Id,
                Name = details.User.Name,
                Email = details.User.Email,
                Role = details.User.Role,
                IsVerifiedProvider = details.User.IsVerifiedProvider
            };

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserEdit(string id, User user)
        {
            if (id != user.Id) return BadRequest();

            // Validation
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Role))
            {
                if (string.IsNullOrEmpty(user.Email))
                    ModelState.AddModelError("Email", "Email is required");
                if (string.IsNullOrEmpty(user.Role))
                    ModelState.AddModelError("Role", "Role is required");
                return View(user);
            }

            // Simple email format validation
            if (!user.Email.Contains("@") || !user.Email.Contains(".com"))
            {
                ModelState.AddModelError("Email", "Invalid email format");
                return View(user);
            }

            // Password optional: if empty, keep existing hash (API handles this)
            // if (string.IsNullOrWhiteSpace(user.PasswordHash)) { ... }

            var success = await _adminApiClient.UpdateUserAsync(id, user);
            if (success)
                return RedirectToAction(nameof(Users));

            ModelState.AddModelError("", "Error updating user");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserDelete(string id)
        {
            var success = await _adminApiClient.DeleteUserAsync(id);
            if (success)
                return RedirectToAction(nameof(Users));

            return BadRequest("Could not delete user");
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

        // Booking edit (ADMIN)
        [HttpGet]
        public async Task<IActionResult> BookingEdit(string id)
        {
            var bookings = await _adminApiClient.GetBookingsAsync();
            var booking = bookings?.FirstOrDefault(b => b.Id == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingEdit(string id, Booking booking)
        {
            if (id != booking.Id) return BadRequest();
            var success = await _adminApiClient.UpdateBookingAsync(id, booking);
            if (success) return RedirectToAction(nameof(Bookings));
            ModelState.AddModelError("", "Error updating booking");
            return View(booking);
        }

        // Booking delete (ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookingDelete(string id)
        {
            var success = await _adminApiClient.DeleteBookingAsync(id);
            if (success) return RedirectToAction(nameof(Bookings));
            return BadRequest("Could not delete booking");
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
        // Global Settings (RF-16)
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var settings = await _settingsApiClient.GetAsync();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(Machly.Web.Models.GlobalSettings model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var success = await _settingsApiClient.UpdateAsync(model);
            if (success)
            {
                TempData["Success"] = "Configuración actualizada correctamente";
                return RedirectToAction(nameof(Settings));
            }
            
            ModelState.AddModelError("", "Error al actualizar la configuración");
            return View(model);
        }

        // Reports (RF-13)
        [HttpGet]
        public async Task<IActionResult> Reports(int? year)
        {
            var selectedYear = year ?? DateTime.Now.Year;
            var income = await _adminApiClient.GetIncomeReportAsync(selectedYear);
            var occupancy = await _adminApiClient.GetOccupancyReportAsync();

            ViewData["Year"] = selectedYear;
            ViewData["Income"] = income;
            ViewData["Occupancy"] = occupancy;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Income()
        {
            var year = DateTime.Now.Year;
            var incomeItems = await _adminApiClient.GetIncomeReportAsync(year);
            
            var dto = new Machly.Web.Models.Admin.IncomeReportDto
            {
                TotalIncome = incomeItems.Sum(i => i.Total),
                Monthly = incomeItems.Select(i => new Machly.Web.Models.Admin.IncomeByMonth 
                { 
                    Month = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i.Month), 
                    Income = i.Total 
                }).ToList()
            };

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Occupancy()
        {
            var occupancyItems = await _adminApiClient.GetOccupancyReportAsync();
            
            var dto = new Machly.Web.Models.Admin.OccupancyReportDto
            {
                Items = occupancyItems.Select(i => new Machly.Web.Models.Admin.MachineOccupancy
                {
                    MachineName = i.MachineTitle,
                    BookingCount = i.Count,
                    OccupancyPercentage = i.Count // Mapping count to percentage field as placeholder
                }).ToList()
            };

            return View(dto);
        }
    }
}

