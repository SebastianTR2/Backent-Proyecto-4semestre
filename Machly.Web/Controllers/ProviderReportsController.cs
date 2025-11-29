using Machly.Web.Services;
using Machly.Web.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Web.Controllers
{
    [Authorize(Roles = "PROVIDER")]
    public class ProviderReportsController : Controller
    {
        private readonly ProviderReportsApiClient _apiClient;

        public ProviderReportsController(ProviderReportsApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> Income(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            var report = await _apiClient.GetIncomeReportAsync(start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(report);
        }

        public async Task<IActionResult> Usage(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            var report = await _apiClient.GetUsageReportAsync(start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(report);
        }

        public async Task<IActionResult> Agro(string scope = "month", DateTime? from = null, DateTime? to = null)
        {
            var (start, end, resolvedScope) = DateRangeHelper.Resolve(scope, from, to);
            var report = await _apiClient.GetAgroReportAsync(start, end);

            ViewData["Scope"] = resolvedScope;
            ViewData["From"] = start?.ToString("yyyy-MM-dd");
            ViewData["To"] = end?.ToString("yyyy-MM-dd");

            return View(report);
        }
    }
}
