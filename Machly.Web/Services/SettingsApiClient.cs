using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class SettingsApiClient
    {
        private readonly HttpClient _httpClient;

        public SettingsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<GlobalSettings?> GetAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<GlobalSettings>("/settings");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> UpdateAsync(GlobalSettings settings)
        {
            var response = await _httpClient.PutAsJsonAsync("/settings", settings);
            return response.IsSuccessStatusCode;
        }
    }
}
