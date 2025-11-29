using System.Security.Claims;
using Machly.Web.Models;
using Machly.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthApiClient _authApiClient;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AuthApiClient authApiClient, ILogger<AccountController> logger)
        {
            _authApiClient = authApiClient;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _authApiClient.LoginAsync(model.Email, model.Password);

            if (!success)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View(model);
            }

            // Después de SignInAsync, el User debería estar disponible
            // Pero para asegurarnos, obtenemos el rol desde los claims
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return role switch
            {
                "ADMIN" => RedirectToAction("Dashboard", "Admin"),
                "PROVIDER" => RedirectToAction("Index", "ProviderMachines"),
                "RENTER" => RedirectToAction("Index", "Renter"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _authApiClient.RegisterAsync(model);

            if (!success)
            {
                ModelState.AddModelError("", "Error al registrar. El email puede estar en uso.");
                return View(model);
            }

            // Después de RegisterAsync (que llama a LoginAsync), obtener el rol
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return role switch
            {
                "ADMIN" => RedirectToAction("Dashboard", "Admin"),
                "PROVIDER" => RedirectToAction("Index", "ProviderMachines"),
                "RENTER" => RedirectToAction("Index", "Renter"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
