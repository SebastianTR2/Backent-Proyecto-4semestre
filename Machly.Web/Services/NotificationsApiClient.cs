using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class NotificationsApiClient
    {
        private readonly HttpClient _httpClient;

        public NotificationsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Notification>> GetByUserAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"/notifications/{userId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Notification>>() ?? new List<Notification>();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId)
        {
            var response = await _httpClient.PutAsync($"/notifications/{notificationId}/read", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendNotificationAsync(NotificationRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/notifications/send", request);
            return response.IsSuccessStatusCode;
        }
    }
}

