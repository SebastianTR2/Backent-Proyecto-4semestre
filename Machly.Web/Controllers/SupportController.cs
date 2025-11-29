using Machly.Web.Models;
using Machly.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Machly.Web.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly SupportApiClient _apiClient;

        public SupportController(SupportApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: /Support
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("ADMIN"))
            {
                var allTickets = await _apiClient.GetAllAsync();
                return View("AdminIndex", allTickets);
            }
            else
            {
                var myTickets = await _apiClient.GetMyTicketsAsync();
                return View(myTickets);
            }
        }

        // GET: /Support/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Support/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _apiClient.CreateAsync(model);
            if (result)
            {
                TempData["Success"] = "Ticket creado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Error al crear el ticket");
            return View(model);
        }

        // POST: /Support/Resolve
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(string id, string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return BadRequest("La respuesta es requerida");

            await _apiClient.ResolveAsync(id, response);
            return RedirectToAction(nameof(Index));
        }
    }
}
