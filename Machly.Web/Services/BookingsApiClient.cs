using System;
using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class BookingsApiClient
    {
        private readonly HttpClient _httpClient;

        public BookingsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Booking?> CreateAsync(string machineId, DateTime start, DateTime end)
        {
            var request = new
            {
                machineId,
                renterId = "", // Se obtiene del JWT en la API
                start,
                end
            };

            var response = await _httpClient.PostAsJsonAsync("/bookings", request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Booking>();
            }
            return null;
        }

        public async Task<List<Booking>> GetByUserAsync(string renterId, DateTime? from = null, DateTime? to = null)
        {
            var query = BuildDateQuery(from, to);
            var response = await _httpClient.GetAsync($"/bookings/user/{renterId}{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Booking>>() ?? new List<Booking>();
        }

        public async Task<List<Booking>> GetByProviderAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = BuildDateQuery(from, to);
            var response = await _httpClient.GetAsync($"/provider/bookings{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Booking>>() ?? new List<Booking>();
        }

        public async Task<bool> CheckInAsync(string bookingId, List<string> photos)
        {
            var request = new { photos };
            var response = await _httpClient.PostAsJsonAsync($"/bookings/{bookingId}/checkin", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckOutAsync(string bookingId, List<string> photos)
        {
            var request = new { photos };
            var response = await _httpClient.PostAsJsonAsync($"/bookings/{bookingId}/checkout", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddReviewAsync(string bookingId, int rating, string comment)
        {
            var request = new { rating, comment };
            var response = await _httpClient.PostAsJsonAsync($"/bookings/review/{bookingId}", request);
            return response.IsSuccessStatusCode;
        }

        private static string BuildDateQuery(DateTime? from, DateTime? to)
        {
            var parameters = new List<string>();
            if (from.HasValue)
            {
                parameters.Add($"from={Uri.EscapeDataString(from.Value.ToUniversalTime().ToString("o"))}");
            }

            if (to.HasValue)
            {
                parameters.Add($"to={Uri.EscapeDataString(to.Value.ToUniversalTime().ToString("o"))}");
            }

            return parameters.Count > 0 ? $"?{string.Join("&", parameters)}" : string.Empty;
        }
    }
}

