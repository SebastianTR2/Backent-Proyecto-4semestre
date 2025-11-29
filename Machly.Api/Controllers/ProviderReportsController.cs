using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("provider/reports")]
    [Authorize(Roles = "PROVIDER")]
    public class ProviderReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ProviderReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("income")]
        public async Task<IActionResult> GetIncomeReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId)) return Unauthorized();

            var report = await _reportService.GetProviderIncomeReport(providerId, from, to);
            return Ok(report);
        }

        [HttpGet("usage")]
        public async Task<IActionResult> GetUsageReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId)) return Unauthorized();

            var report = await _reportService.GetProviderUsageReport(providerId, from, to);
            return Ok(report);
        }

        [HttpGet("services-agro")]
        public async Task<IActionResult> GetAgroReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId)) return Unauthorized();

            var report = await _reportService.GetProviderAgroReport(providerId, from, to);
            return Ok(report);
        }
    }
}
