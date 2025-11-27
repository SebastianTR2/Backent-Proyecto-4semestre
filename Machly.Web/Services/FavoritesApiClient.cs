using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class FavoritesApiClient
    {
        private readonly HttpClient _httpClient;

        public FavoritesApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> AddFavoriteAsync(string machineId)
        {
            var response = await _httpClient.PostAsync($"/favorites/{machineId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveFavoriteAsync(string machineId)
        {
            var response = await _httpClient.DeleteAsync($"/favorites/{machineId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Favorite>> GetUserFavoritesAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/favorites/user/{userId}");
            if (!response.IsSuccessStatusCode) return new List<Favorite>();

            return await response.Content.ReadFromJsonAsync<List<Favorite>>() ?? new List<Favorite>();
        }
    }
}
