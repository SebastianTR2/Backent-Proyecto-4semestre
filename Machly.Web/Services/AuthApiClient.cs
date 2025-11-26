using Machly.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Machly.Web.Services
{
    public class AuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _context;

        public AuthApiClient(
            HttpClient httpClient,
            IHttpContextAccessor context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        // =============================================================
        // 1️⃣ LOGIN QUE RETORNA SOLO EL TOKEN
        // =============================================================
        public async Task<string?> LoginAndGetTokenAsync(string email, string password)
        {
            var request = new { email, password };

            var response = await _httpClient.PostAsJsonAsync("/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!json.TryGetProperty("token", out var tokenElement))
                return null;

            return tokenElement.GetString();
        }

        // =============================================================
        // 2️⃣ LOGIN COMPLETO QUE CREA COOKIE MVC
        // =============================================================
        public async Task<bool> LoginAsync(string email, string password)
        {
            var token = await LoginAndGetTokenAsync(email, password);
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Claims que vamos a guardar en la cookie de autenticación MVC
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, jwt.Claims.First(c => c.Type == "id").Value),
                new Claim(ClaimTypes.Email,       jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value),
                new Claim(ClaimTypes.Role,        jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value),
                new Claim("access_token", token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7),
            };

            // Guardar el token en los properties para que esté disponible en GetTokenAsync
            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = token }
            });

            await _context.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);

            return true;
        }

        // =============================================================
        // 3️⃣ REGISTRO
        // =============================================================
        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            var request = new
            {
                name = model.Name,
                email = model.Email,
                password = model.Password,
                role = model.Role
            };

            var response = await _httpClient.PostAsJsonAsync("/auth/register", request);
            if (!response.IsSuccessStatusCode)
                return false;

            return await LoginAsync(model.Email, model.Password);
        }
    }
}
