using System;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("provider")]
    [Authorize(Roles = "PROVIDER")]
    public class ProviderController : ControllerBase
    {
        private readonly MachineService _machineService;
        private readonly BookingService _bookingService;

        public ProviderController(MachineService machineService, BookingService bookingService)
        {
            _machineService = machineService;
            _bookingService = bookingService;
        }

        // GET /provider/machines - Máquinas del proveedor logueado
        [HttpGet("machines")]
        public async Task<IActionResult> GetMachines()
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var machines = await _machineService.GetByProviderAsync(providerId);
            return Ok(machines);
        }

        // PUT /provider/machines/{id} - Editar máquina del proveedor
        [HttpPut("machines/{id}")]
        public async Task<IActionResult> UpdateMachine(string id, [FromBody] Machly.Api.Models.Machine machine)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var existing = await _machineService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (existing.ProviderId != providerId)
                return Forbid();

            machine.Id = id;
            machine.ProviderId = providerId;

            var result = await _machineService.UpdateAsync(id, machine);
            if (!result)
                return BadRequest("Failed to update machine");

            return Ok(machine);
        }

        // DELETE /provider/machines/{id} - Eliminar máquina del proveedor
        [HttpDelete("machines/{id}")]
        public async Task<IActionResult> DeleteMachine(string id)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var existing = await _machineService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            if (existing.ProviderId != providerId)
                return Forbid();

            var result = await _machineService.DeleteAsync(id);
            if (!result)
                return BadRequest("Failed to delete machine");

            return Ok(new { message = "Machine deleted successfully" });
        }

        // GET /provider/bookings - Reservas de las máquinas del proveedor
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var providerId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(providerId))
                return Unauthorized();

            var bookings = await _bookingService.GetByProviderAsync(providerId, from, to);
            return Ok(bookings);
        }
    }
}

