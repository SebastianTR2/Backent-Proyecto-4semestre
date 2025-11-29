using Machly.Web.Models;
using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Machly.Web.Controllers
{
    [Authorize(Roles = "RENTER")]
    public class RenterController : Controller
    {
        private readonly BookingsApiClient _bookingsApiClient;
        private readonly FavoritesApiClient _favoritesApiClient;
        private readonly MachinesApiClient _machinesApiClient;

        public RenterController(
            BookingsApiClient bookingsApiClient,
            FavoritesApiClient favoritesApiClient,
            MachinesApiClient machinesApiClient)
        {
            _bookingsApiClient = bookingsApiClient;
            _favoritesApiClient = favoritesApiClient;
            _machinesApiClient = machinesApiClient;
        }

        public async Task<IActionResult> Index(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            List<BookingDetailDto> bookings = await _bookingsApiClient.GetByUserAsync(userId, start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(bookings);
        }

        public async Task<IActionResult> Explore(string? category, string? type, decimal? maxPrice)
        {
            var machines = await _machinesApiClient.GetAllAsync(maxPrice: maxPrice, type: type);
            
            if (!string.IsNullOrEmpty(category))
            {
                machines = machines.Where(m => m.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (!string.IsNullOrEmpty(userId))
            {
                var favorites = await _favoritesApiClient.GetUserFavoritesAsync(userId);
                ViewData["Favorites"] = favorites.Select(f => f.MachineId).ToHashSet();
            }
            else
            {
                ViewData["Favorites"] = new HashSet<string>();
            }

            ViewData["Category"] = category;
            ViewData["Type"] = type;
            ViewData["MaxPrice"] = maxPrice;

            return View(machines);
        }

        public async Task<IActionResult> Details(string id)
        {
            var machine = await _machinesApiClient.GetByIdAsync(id);
            if (machine == null) return NotFound();
            return View(machine);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBooking(Machly.Web.Models.ViewModels.CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // If invalid, we need to show the Details view again. 
                // However, Details view expects a Machine model, not CreateBookingViewModel.
                // We need to fetch the machine details again.
                var machine = await _machinesApiClient.GetByIdAsync(model.MachineId);
                if (machine == null) return NotFound();
                
                // We might want to pass the model back via ViewBag or similar if we want to preserve inputs,
                // but for now let's just return the view with the machine and validation errors.
                return View("Details", machine);
            }

            // Ensure RenterId is set from the logged-in user
            model.RenterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

            var ok = await _bookingsApiClient.CreateBookingAsync(model);

            if (!ok)
            {
                TempData["Error"] = "No se pudo completar la reserva.";
                var machine = await _machinesApiClient.GetByIdAsync(model.MachineId);
                return View("Details", machine);
            }

            TempData["Success"] = "Reserva realizada con éxito.";
            return RedirectToAction("Index"); // Redirect to My Bookings
        }

        public async Task<IActionResult> Favorites()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var favorites = await _favoritesApiClient.GetUserFavoritesAsync(userId);
            var machines = new List<Machly.Web.Models.Machine>();

            foreach (var fav in favorites)
            {
                var machine = await _machinesApiClient.GetByIdAsync(fav.MachineId);
                if (machine != null)
                {
                    machines.Add(machine);
                }
            }

            return View(machines);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(string machineId, string returnUrl = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var favorites = await _favoritesApiClient.GetUserFavoritesAsync(userId);
            var exists = favorites.Any(f => f.MachineId == machineId);

            if (exists)
            {
                await _favoritesApiClient.RemoveFavoriteAsync(machineId);
            }
            else
            {
                await _favoritesApiClient.AddFavoriteAsync(machineId);
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Favorites));
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
        [HttpGet]
        public IActionResult Map()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNearMachines(
            double? lat, double? lng, double? radiusKm,
            decimal? maxPrice, string? type, string? category, bool? withOperator)
        {
            var machines = await _machinesApiClient.GetAllAsync(
                lat: lat, lng: lng, radius: radiusKm,
                maxPrice: maxPrice, type: type);
            
            // Nota: MachinesApiClient.GetAllAsync original no tenía category ni withOperator.
            // Deberíamos actualizar MachinesApiClient si queremos filtrar por eso en el servidor.
            // Por ahora, filtramos en memoria lo que falte o actualizamos el cliente.
            // Vamos a filtrar en memoria lo que falte para no romper la firma si es compleja,
            // o mejor, actualizamos el cliente.
            
            if (!string.IsNullOrEmpty(category))
            {
                machines = machines.Where(m => m.Category.Contains(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (withOperator.HasValue)
            {
                machines = machines.Where(m => m.WithOperator == withOperator.Value).ToList();
            }

            return Json(machines);
        }
    }
}
