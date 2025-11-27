using Machly.Web.Models;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class SupportApiClient
    {
        private readonly HttpClient _httpClient;

        public SupportApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> CreateAsync(CreateTicketViewModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("/support", new SupportTicket
            {
                Subject = model.Subject,
                Message = model.Message,
                Priority = model.Priority
            });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<SupportTicket>> GetMyTicketsAsync()
        {
            var response = await _httpClient.GetAsync("/support/my-tickets");
            if (!response.IsSuccessStatusCode) return new List<SupportTicket>();
            return await response.Content.ReadFromJsonAsync<List<SupportTicket>>() ?? new List<SupportTicket>();
        }

        public async Task<List<SupportTicket>> GetAllAsync()
        {
            var response = await _httpClient.GetAsync("/support/all");
            if (!response.IsSuccessStatusCode) return new List<SupportTicket>();
            return await response.Content.ReadFromJsonAsync<List<SupportTicket>>() ?? new List<SupportTicket>();
        }

        public async Task<bool> ResolveAsync(string id, string responseText)
        {
            var response = await _httpClient.PutAsJsonAsync($"/support/{id}/resolve", new { Response = responseText });
            return response.IsSuccessStatusCode;
        }
    }
}
