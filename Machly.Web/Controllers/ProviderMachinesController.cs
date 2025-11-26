using Machly.Web.Models;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    [Authorize(Policy = "ProviderOnly")]
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
            var machines = await _machinesApiClient.GetByProviderAsync();
            return View(machines);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Iniciar estructura vacía
            return View(new Machine
            {
                TariffsAgro = new TariffAgro
                {
                    KmTariffs = new List<KmTariff>
                    {
                        new KmTariff { MinKm = 0, MaxKm = 9999, TarifaPorKm = 5m }
                    }
                },
                Photos = new List<MachinePhoto>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Machine machine)
        {
            if (!ModelState.IsValid)
                return View(machine);

            // Asegurar estructura existente
            FixTariffs(machine);

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

            // Asegurar estructura válida
            FixTariffs(machine);

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

            FixTariffs(machine);

            var result = await _machinesApiClient.UpdateAsync(id, machine);
            if (result)
                return RedirectToAction(nameof(Index));

            ModelState.AddModelError("", "Error al actualizar la máquina");
            return View(machine);
        }

        private void FixTariffs(Machine machine)
        {
            if (machine.TariffsAgro == null)
                machine.TariffsAgro = new TariffAgro();

            if (machine.TariffsAgro.KmTariffs == null)
                machine.TariffsAgro.KmTariffs = new List<KmTariff>();

            // mínimo 1 tramo
            if (!machine.TariffsAgro.KmTariffs.Any())
            {
                machine.TariffsAgro.KmTariffs.Add(new KmTariff
                {
                    MinKm = 0,
                    MaxKm = 9999,
                    TarifaPorKm = 5m
                });
            }
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
            var bookings = await _bookingsApiClient.GetByProviderAsync(start, end);
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
    }
}
