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
        private readonly BookingService _bookingService;

        public MachinesController(MachineService service, BookingService bookingService)
        {
            _service = service;
            _bookingService = bookingService;
        }

        // GET /machines/{id}/calendar
        [HttpGet("{id}/calendar")]
        public async Task<IActionResult> GetCalendar(string id)
        {
            var machine = await _service.GetByIdAsync(id);
            if (machine == null) return NotFound();

            var bookings = await _bookingService.GetByMachineAsync(id);
            
            // Filtrar reservas activas
            var activeBookings = bookings
                .Where(b => b.Status == "CONFIRMED" || b.Status == "IN_PROGRESS")
                .Select(b => new
                {
                    Start = b.Start,
                    End = b.End,
                    Type = "BOOKING",
                    Title = "Reservado"
                });

            // Bloqueos manuales
            var manualBlocks = machine.Calendar.Select(c => new
            {
                Start = c.Start,
                End = c.End,
                Type = c.Type, // BLOCKED, MAINTENANCE
                Title = c.Type == "MAINTENANCE" ? "Mantenimiento" : "No Disponible"
            });

            var calendar = activeBookings.Concat(manualBlocks).ToList();
            return Ok(calendar);
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

        // GET /machines/near
        [HttpGet("near")]
        public async Task<IActionResult> GetNear([FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radiusKm = 50)
        {
            var machines = await _service.GetFilteredAsync(lat: lat, lng: lng, radiusKm: radiusKm);
            return Ok(machines);
        }

        // GET /machines/{id} - Público
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var machine = await _service.GetByIdAsync(id);
            if (machine == null) return NotFound();
            return Ok(machine);
        }

        // GET /machines/provider/{providerId}
        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetByProvider(string providerId)
        {
            var machines = await _service.GetByProviderAsync(providerId);
            return Ok(machines);
        }

        // POST /machines - PROVIDER or ADMIN
        [HttpPost]
        [Authorize(Roles = "ADMIN,PROVIDER")]
        public async Task<IActionResult> Create([FromBody] Machine machine)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (role == "PROVIDER")
            {
                machine.ProviderId = userId;
            }
            else if (role == "ADMIN")
            {
                // Admin must provide ProviderId or we assign it to Admin (if Admin is also a user)
                // For now, if ProviderId is missing, we can assign it to the Admin's ID or return BadRequest.
                // Assuming Admin can create machines for themselves or others if specified.
                if (string.IsNullOrEmpty(machine.ProviderId))
                {
                    machine.ProviderId = userId;
                }
            }

            await _service.CreateAsync(machine);
            return Ok(machine);
        }

        // PUT /machines/{id} - PROVIDER (Owner) or ADMIN
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,PROVIDER")]
        public async Task<IActionResult> Update(string id, [FromBody] Machine machine)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Check permissions
            if (role != "ADMIN" && existing.ProviderId != userId)
            {
                return Forbid();
            }

            machine.Id = id;
            // Preserve ProviderId if not Admin, or if Admin didn't change it (optional logic)
            // For safety, if not Admin, force ProviderId to remain same
            if (role != "ADMIN")
            {
                machine.ProviderId = existing.ProviderId;
            }
            else if (string.IsNullOrEmpty(machine.ProviderId))
            {
                machine.ProviderId = existing.ProviderId;
            }

            var result = await _service.UpdateAsync(id, machine);
            if (!result) return BadRequest("Failed to update machine");

            return Ok(machine);
        }

        // DELETE /machines/{id} - PROVIDER (Owner) or ADMIN
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,PROVIDER")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var existing = await _service.GetByIdAsync(id);
            if (existing == null) return NotFound();

            if (role != "ADMIN" && existing.ProviderId != userId)
            {
                return Forbid();
            }

            var result = await _service.DeleteAsync(id);
            if (!result) return BadRequest("Failed to delete machine");

            return Ok(new { message = "Machine deleted successfully" });
        }

        // PUT /machines/{id}/status - PROVIDER (Owner) or ADMIN
        [HttpPut("{id}/status")]
        [Authorize(Roles = "ADMIN,PROVIDER")]
        public async Task<IActionResult> ToggleStatus(string id, [FromBody] bool isOutOfService)
        {
            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var machine = await _service.GetByIdAsync(id);
            if (machine == null) return NotFound();

            if (role != "ADMIN" && machine.ProviderId != userId) return Forbid();

            machine.IsOutOfService = isOutOfService;
            await _service.UpdateAsync(id, machine);

            return Ok(machine);
        }

        // POST /machines/import - Solo ADMIN
        [HttpPost("import")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Archivo vacío");

            var machines = new List<Machine>();
            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                // Ignorar header
                await stream.ReadLineAsync();
                
                while (!stream.EndOfStream)
                {
                    var line = await stream.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length >= 5)
                    {
                        // Formato: Title,Type,Category,Price,ProviderId
                        var m = new Machine
                        {
                            Title = parts[0].Trim(),
                            Type = parts[1].Trim(),
                            Category = parts[2].Trim(),
                            PricePerDay = decimal.Parse(parts[3]),
                            ProviderId = parts[4].Trim(),
                            Description = "Importada masivamente"
                        };
                        machines.Add(m);
                    }
                }
            }

            foreach (var m in machines)
            {
                await _service.CreateAsync(m);
            }

            return Ok(new { Count = machines.Count });
        }

        // POST /machines/simulate-tariff
        [HttpPost("simulate-tariff")]
        public async Task<IActionResult> SimulateTariff([FromBody] TariffSimulationRequest request)
        {
            var machine = await _service.GetByIdAsync(request.MachineId);
            if (machine == null) return NotFound();

            decimal total = 0;
            decimal subHectareas = 0;
            decimal subToneladas = 0;
            decimal subKm = 0;

            if (machine.TariffsAgro != null)
            {
                if (request.Hectareas.HasValue && machine.TariffsAgro.Hectarea.HasValue)
                {
                    subHectareas = (decimal)request.Hectareas.Value * machine.TariffsAgro.Hectarea.Value;
                }

                if (request.Toneladas.HasValue && machine.TariffsAgro.Tonelada.HasValue)
                {
                    subToneladas = (decimal)request.Toneladas.Value * machine.TariffsAgro.Tonelada.Value;
                }

                if (request.Km.HasValue && machine.TariffsAgro.KmTariffs != null)
                {
                    // Buscar tramo
                    var tramo = machine.TariffsAgro.KmTariffs
                        .FirstOrDefault(t => request.Km.Value >= t.MinKm && request.Km.Value <= t.MaxKm);
                    
                    if (tramo != null)
                    {
                        subKm = (decimal)request.Km.Value * tramo.TarifaPorKm;
                    }
                }
            }

            total = subHectareas + subToneladas + subKm + machine.PricePerDay; // + precio base diario

            return Ok(new
            {
                SubtotalHectareas = subHectareas,
                SubtotalToneladas = subToneladas,
                SubtotalKm = subKm,
                BasePrice = machine.PricePerDay,
                Total = total
            });
        }
    }

    public class TariffSimulationRequest
    {
        public string MachineId { get; set; } = null!;
        public double? Hectareas { get; set; }
        public double? Toneladas { get; set; }
        public double? Km { get; set; }
    }
}
