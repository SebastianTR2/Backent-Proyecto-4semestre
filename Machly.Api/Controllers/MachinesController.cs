using Machly.Api.Models;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("machines")]
    public class MachinesController : ControllerBase
    {
        private readonly MachineService _service;

        public MachinesController(MachineService service)
        {
            _service = service;
        }

        // GET /machines - Público, con filtros opcionales
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] double? lat,
            [FromQuery] double? lng,
            [FromQuery] double? radius,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? type,
            [FromQuery] string? category,
            [FromQuery] bool? withOperator)
        {
            if (lat.HasValue || lng.HasValue || radius.HasValue || minPrice.HasValue || maxPrice.HasValue ||
                !string.IsNullOrEmpty(type) || !string.IsNullOrEmpty(category) || withOperator.HasValue)
            {
                var machines = await _service.GetFilteredAsync(
                    lat, lng, radius, minPrice, maxPrice, type, category, withOperator);
                return Ok(machines);
            }

            return Ok(await _service.GetAllAsync());
        }

        // GET /machines/{id} - Público
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var machine = await _service.GetByIdAsync(id);
            if (machine == null) return NotFound();
            return Ok(machine);
        }

        // POST /machines - Solo PROVIDER
        [HttpPost]
        [Authorize(Roles = "PROVIDER")]
        public async Task<IActionResult> Create([FromBody] Machine machine)
        {
            // Obtener ProviderId del JWT
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            machine.ProviderId = providerId;
            await _service.CreateAsync(machine);
            return Ok(machine);
        }
    }
}
