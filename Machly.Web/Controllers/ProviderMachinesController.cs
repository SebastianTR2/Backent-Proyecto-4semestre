using Machly.Web.Models;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    [Authorize(Roles = "PROVIDER")]
    public class ProviderMachinesController : Controller
    {
        private readonly MachinesApiClient _machinesApiClient;
        private readonly BookingsApiClient _bookingsApiClient;

        public ProviderMachinesController(
            MachinesApiClient machinesApiClient,
            BookingsApiClient bookingsApiClient)
        {
            _machinesApiClient = machinesApiClient;
            _bookingsApiClient = bookingsApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var machines = await _machinesApiClient.GetByProviderAsync(userId);
            return View(machines);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Iniciar estructura vacía
            return View(new Machine
            {
                TariffsAgro = new TariffAgro(),
                Photos = new List<MachinePhoto>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Machine machine)
        {
            if (!ModelState.IsValid)
                return View(machine);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            
            machine.ProviderId = userId; // Asegurar providerId

            var result = await _machinesApiClient.CreateAsync(machine);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al crear la máquina");
            return View(machine);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var machine = await _machinesApiClient.GetByIdAsync(id);
            if (machine == null)
                return NotFound();

            return View(machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Machine machine)
        {
            if (id != machine.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(machine);

            var result = await _machinesApiClient.UpdateAsync(id, machine);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al actualizar la máquina");
            return View(machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _machinesApiClient.DeleteAsync(id);
            if (result)
                return RedirectToAction(nameof(Index));

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Bookings(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            List<BookingDetailDto> bookings = await _bookingsApiClient.GetByProviderAsync(start, end);
            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");
            return View(bookings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string bookingId, List<string> photos)
        {
            var result = await _bookingsApiClient.CheckInAsync(bookingId, photos);
            if (result)
                return RedirectToAction(nameof(Bookings));

            return BadRequest();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string bookingId, List<string> photos)
        {
            var result = await _bookingsApiClient.CheckOutAsync(bookingId, photos);
            if (result)
                return RedirectToAction(nameof(Bookings));

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Calendar(string id)
        {
            var machine = await _machinesApiClient.GetByIdAsync(id);
            if (machine == null) return NotFound();

            var events = await _machinesApiClient.GetCalendarAsync(id);
            ViewData["MachineTitle"] = machine.Title;
            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(string id, bool isOutOfService)
        {
            var result = await _machinesApiClient.ToggleStatusAsync(id, isOutOfService);
            if (result)
                return RedirectToAction(nameof(Index));

            return BadRequest();
        }
        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyMachinesGeo()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var machines = await _machinesApiClient.GetByProviderAsync(userId);
            return Json(machines);
        }
        [HttpGet]
        public IActionResult TariffSimulator()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Simulate([FromBody] object data)
        {
            // Proxy manual rápido usando HttpClient
            // Idealmente usar MachinesApiClient
            // Vamos a agregar SimulateAsync a MachinesApiClient
            var result = await _machinesApiClient.SimulateTariffAsync(data);
            return Json(result);
        }
    }
}
