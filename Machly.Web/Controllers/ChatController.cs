using Machly.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Machly.Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MachlyApi");
        }

        public IActionResult Index(string providerId)
        {
            return View("~/Views/Renter/Chat.cshtml", providerId);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(string otherUserId)
        {
            // Proxy a la API
            // Necesitamos pasar el token del usuario actual
            // Asumimos que el HttpClient ya tiene el handler de token o lo añadimos aquí
            // Si usamos JwtDelegatingHandler registrado en Program.cs, el token se adjunta solo.
            
            var response = await _httpClient.GetAsync($"/chat/history/{otherUserId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            return BadRequest();
        }
    }
}
