using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Si el usuario está autenticado, redirigir según su rol
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                
                return role switch
                {
                    "ADMIN" => RedirectToAction("Dashboard", "Admin"),
                    "PROVIDER" => RedirectToAction("Index", "ProviderMachines"),
                    "RENTER" => RedirectToAction("Index", "Renter"),
                    _ => View() // Si no tiene rol, mostrar landing
                };
            }
            
            // Si no está autenticado, mostrar landing page
            return View();
        }
    }
}
