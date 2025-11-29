using Machly.Web.Models.Reports;
using System.Net.Http.Json;

namespace Machly.Web.Services
{
    public class ProviderReportsApiClient
    {
        private readonly HttpClient _httpClient;

        public ProviderReportsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProviderIncomeReportDto> GetIncomeReportAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = BuildQuery(from, to);
            return await _httpClient.GetFromJsonAsync<ProviderIncomeReportDto>($"/provider/reports/income{query}") 
                   ?? new ProviderIncomeReportDto();
        }

        public async Task<ProviderUsageReportDto> GetUsageReportAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = BuildQuery(from, to);
            return await _httpClient.GetFromJsonAsync<ProviderUsageReportDto>($"/provider/reports/usage{query}") 
                   ?? new ProviderUsageReportDto();
        }

        public async Task<ProviderAgroReportDto> GetAgroReportAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = BuildQuery(from, to);
            return await _httpClient.GetFromJsonAsync<ProviderAgroReportDto>($"/provider/reports/services-agro{query}") 
                   ?? new ProviderAgroReportDto();
        }

        private string BuildQuery(DateTime? from, DateTime? to)
        {
            var parts = new List<string>();
            if (from.HasValue) parts.Add($"from={from.Value:yyyy-MM-dd}");
            if (to.HasValue) parts.Add($"to={to.Value:yyyy-MM-dd}");
            
            return parts.Any() ? "?" + string.Join("&", parts) : "";
        }
    }
}
