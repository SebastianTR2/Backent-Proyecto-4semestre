using System;
using System.Globalization;
using Machly.Web.Models;
using Machly.Web.Models.Admin;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class AdminApiClient
    {
        private readonly HttpClient _httpClient;

        // Constructor CORRECTO ? HttpClient ya viene con delegating handler
        public AdminApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ==========================
        // GET: /admin/users
        // ==========================
        public async Task<List<User>> GetUsersAsync()
        {
            var response = await _httpClient.GetAsync("/admin/users");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<User>>()
                   ?? new List<User>();
        }

        // ==========================
        // GET: /admin/machines
        // ==========================
        public async Task<AdminUserDetailsDto?> GetUserDetailsAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/admin/users/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AdminUserDetailsDto>();
        }

        public async Task<List<AdminMachineListItem>> GetMachinesAsync(string? type = null, string? category = null, string? providerId = null)
        {
            var query = BuildQuery(new Dictionary<string, string?>
            {
                ["type"] = type,
                ["category"] = category,
                ["providerId"] = providerId
            });

            var response = await _httpClient.GetAsync($"/admin/machines{query}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<AdminMachineListItem>>()
                   ?? new List<AdminMachineListItem>();
        }

        // ==========================
        // GET: /admin/bookings
        // ==========================
        public async Task<AdminMachineDetailsDto?> GetMachineDetailsAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/admin/machines/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AdminMachineDetailsDto>();
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/admin/machines/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Booking>> GetBookingsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = BuildQuery(new Dictionary<string, string?>
            {
                ["from"] = from.HasValue ? from.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) : null,
                ["to"] = to.HasValue ? to.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) : null
            });

            var response = await _httpClient.GetAsync($"/admin/bookings{query}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Booking>>()
                   ?? new List<Booking>();
        }

        // ==========================
        // PUT: /admin/provider/verify/{userId}
        // ==========================
        public async Task<bool> VerifyProviderAsync(string userId, bool isVerified)
        {
            var response = await _httpClient.PutAsJsonAsync($"/admin/provider/verify/{userId}", new
            {
                IsVerified = isVerified
            });
            return response.IsSuccessStatusCode;
        }

        // ==========================
        // GET: /admin/reports/basic
        // ==========================
        public async Task<object> GetBasicReportsAsync()
        {
            var response = await _httpClient.GetAsync("/admin/reports/basic");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<object>()
                   ?? new { };
        }

        private static string BuildQuery(Dictionary<string, string?> parameters)
        {
            var parts = parameters
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}")
                .ToList();

            return parts.Count > 0 ? $"?{string.Join("&", parts)}" : string.Empty;
        }
    }
}
