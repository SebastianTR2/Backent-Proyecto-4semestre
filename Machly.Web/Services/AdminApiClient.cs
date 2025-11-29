using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Machly.Web.Models;
using Machly.Web.Models.Admin;
using Microsoft.AspNetCore.Http;

namespace Machly.Web.Services
{
    public class AdminApiClient
    {
        private readonly HttpClient _httpClient;

        public AdminApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Users
        public async Task<List<ApiAdminUserSummaryDto>> GetUsersAsync(string? role = null)
        {
            var url = "/admin/users";
            if (!string.IsNullOrEmpty(role)) url += $"?role={role}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ApiAdminUserSummaryDto>>() ?? new List<ApiAdminUserSummaryDto>();
        }

        public async Task<AdminUserDetailsDto?> GetUserDetailsAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/admin/users/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminUserDetailsDto>();
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("/admin/users", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> GetCreateUserResultAsync(User user)
        {
            var response = await _httpClient.PostAsJsonAsync("/admin/users", user);
            if (response.IsSuccessStatusCode)
                return (true, null);
            
            var error = await response.Content.ReadAsStringAsync();
            return (false, error);
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            var response = await _httpClient.PutAsJsonAsync($"/admin/users/{id}", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/admin/users/{id}");
            return response.IsSuccessStatusCode;
        }

        // Machines list
        public async Task<List<AdminMachineListItem>> GetMachinesAsync(string? type = null, string? category = null, string? providerId = null, bool? withOperator = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(type)) queryParams.Add($"type={type}");
            if (!string.IsNullOrEmpty(category)) queryParams.Add($"category={category}");
            if (!string.IsNullOrEmpty(providerId)) queryParams.Add($"providerId={providerId}");
            if (withOperator.HasValue) queryParams.Add($"withOperator={withOperator}");
            if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice}");
            if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice}");
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var response = await _httpClient.GetAsync($"/admin/machines{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AdminMachineListItem>>() ?? new List<AdminMachineListItem>();
        }

        public async Task<AdminMachineDetailsDto?> GetMachineDetailsAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/admin/machines/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AdminMachineDetailsDto>();
        }

        public async Task<bool> DeleteMachineAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/admin/machines/{id}");
            return response.IsSuccessStatusCode;
        }

        // Bookings list
        public async Task<List<AdminBookingListItem>> GetBookingsAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = string.Empty;
            if (from.HasValue && to.HasValue)
                query = $"?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
            var response = await _httpClient.GetAsync($"/admin/bookings{query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AdminBookingListItem>>() ?? new List<AdminBookingListItem>();
        }

        public async Task<bool> UpdateBookingAsync(string id, Booking booking)
        {
            var response = await _httpClient.PutAsJsonAsync($"/bookings/{id}", booking);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteBookingAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"/bookings/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> VerifyProviderAsync(string id, bool isVerified)
        {
            var response = await _httpClient.PutAsJsonAsync($"/admin/provider/verify/{id}", new { IsVerified = isVerified });
            return response.IsSuccessStatusCode;
        }

        public async Task<AdminDashboardDto?> GetDashboardStatsAsync()
        {
            var response = await _httpClient.GetAsync("/admin/dashboard");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AdminDashboardDto>();
        }

        public async Task<List<AuditLog>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? userId = null, string? action = null)
        {
            var queryParams = new List<string>();
            if (from.HasValue) queryParams.Add($"from={from:yyyy-MM-dd}");
            if (to.HasValue) queryParams.Add($"to={to:yyyy-MM-dd}");
            if (!string.IsNullOrEmpty(userId)) queryParams.Add($"userId={userId}");
            if (!string.IsNullOrEmpty(action)) queryParams.Add($"action={action}");
            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;
            var response = await _httpClient.GetAsync($"/admin/logs{queryString}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AuditLog>>() ?? new List<AuditLog>();
        }

        public async Task<byte[]> GetUsersCsvAsync()
        {
            return await _httpClient.GetByteArrayAsync("/admin/export/users/csv");
        }

        public async Task<byte[]> GetMachinesCsvAsync()
        {
            return await _httpClient.GetByteArrayAsync("/admin/export/machines/csv");
        }

        public async Task<bool> ImportMachinesAsync(MultipartFormDataContent content)
        {
            var response = await _httpClient.PostAsync("/machines/import", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ImportMachinesAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            return await ImportMachinesAsync(content);
        }
        public async Task<List<IncomeReportItem>> GetIncomeReportAsync(int year)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<IncomeReportItem>>($"/admin/reports/income?year={year}") ?? new List<IncomeReportItem>();
            }
            catch
            {
                return new List<IncomeReportItem>();
            }
        }

        public async Task<List<OccupancyReportItem>> GetOccupancyReportAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<OccupancyReportItem>>("/admin/reports/occupancy") ?? new List<OccupancyReportItem>();
            }
            catch
            {
                return new List<OccupancyReportItem>();
            }
        }
    }

    public class IncomeReportItem
    {
        public int Month { get; set; }
        public decimal Total { get; set; }
    }

    public class OccupancyReportItem
    {
        public string MachineTitle { get; set; }
        public int Count { get; set; }
    }
}
