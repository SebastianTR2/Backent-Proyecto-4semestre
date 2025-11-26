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

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"/machines{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Machine>>() ?? new List<Machine>();
        }

        public async Task<Machine?> GetByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/machines/{id}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Machine>();
        }

        public async Task<bool> CreateAsync(Machine machine)
        {
            var response = await _httpClient.PostAsJsonAsync("/machines", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(string id, Machine machine)
        {
            var response = await _httpClient.PutAsJsonAsync($"/provider/machines/{id}", machine);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/provider/machines/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Machine>> GetByProviderAsync()
        {
            var response = await _httpClient.GetAsync("/provider/machines");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Machine>>() ?? new List<Machine>();
        }
    }
}

