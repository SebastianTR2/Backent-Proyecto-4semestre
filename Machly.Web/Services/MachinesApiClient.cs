using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class MachinesApiClient
    {
        private readonly HttpClient _httpClient;

        public MachinesApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ============================
        // 🔵 1. GET ALL (con filtros)
        // ============================
        public async Task<List<Machine>> GetAllAsync(
            double? lat = null,
            double? lng = null,
            double? radius = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? type = null)
        {
            var queryParams = new List<string>();

            if (lat.HasValue) queryParams.Add($"lat={lat.Value}");
            if (lng.HasValue) queryParams.Add($"lng={lng.Value}");
            if (radius.HasValue) queryParams.Add($"radius={radius.Value}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
            if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={type}");

            var queryString = queryParams.Any()
                ? "?" + string.Join("&", queryParams)
                : "";

            var response = await _httpClient.GetAsync($"/machines{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Machine>>() ?? new List<Machine>();
        }

        // ============================
        // 🔵 2. GET BY ID
        // ============================
        public async Task<Machine?> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/machines/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Machine>();
        }

        // ============================
        // 🟢 3. CREATE (Provider)
        // ============================
        public async Task<bool> CreateAsync(Machine machine)
        {
            var response = await _httpClient.PostAsJsonAsync("/machines", machine);
            return response.IsSuccessStatusCode;
        }

        // ============================
        // 🟡 4. UPDATE (Provider)
        // ============================
        public async Task<bool> UpdateAsync(string id, Machine machine)
        {
            var response = await _httpClient.PutAsJsonAsync($"/provider/machines/{id}", machine);
            return response.IsSuccessStatusCode;
        }

        // ============================
        // 🔴 5. DELETE (Provider/Admin)
        // ============================
        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/provider/machines/{id}");
            return response.IsSuccessStatusCode;
        }

        // ============================
        // 🔵 6. GET BY PROVIDER
        // ============================
        public async Task<List<Machine>> GetByProviderAsync(string providerId)
        {
            var response = await _httpClient.GetAsync($"/machines/provider/{providerId}");
            if (!response.IsSuccessStatusCode)
                return new List<Machine>();

            return await response.Content.ReadFromJsonAsync<List<Machine>>() ?? new List<Machine>();
        }

        // ============================
        // 🔵 7. GET CALENDAR
        // ============================
        public async Task<List<CalendarEventDto>> GetCalendarAsync(string machineId)
        {
            var response = await _httpClient.GetAsync($"/machines/{machineId}/calendar");
            if (!response.IsSuccessStatusCode)
                return new List<CalendarEventDto>();

            return await response.Content.ReadFromJsonAsync<List<CalendarEventDto>>() ?? new List<CalendarEventDto>();
        }

        // ============================
        // 🔵 8. TOGGLE STATUS (Servicio / fuera de servicio)
        // ============================
        public async Task<bool> ToggleStatusAsync(string id, bool isOutOfService)
        {
            var response = await _httpClient.PutAsJsonAsync($"/machines/{id}/status", isOutOfService);
            return response.IsSuccessStatusCode;
        }
        public async Task<object?> SimulateTariffAsync(object data)
        {
            var response = await _httpClient.PostAsJsonAsync("/machines/simulate-tariff", data);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<object>();
        }
    }
}
