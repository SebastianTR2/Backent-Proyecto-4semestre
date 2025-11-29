using Machly.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("agro")]
    public class AgroController : ControllerBase
    {
        [HttpPost("calculate")]
        public IActionResult Calculate([FromBody] AgroCalculateRequest request)
        {
            // MOCK Sprint 1: cálculos simples basados en valores por defecto
            const double precioHectarea = 350;   // Bs/ha
            const double precioTonelada = 45;    // Bs/ton
            const double tarifaKm = 0.9;         // Bs/km*ton (mock)

            double total = 0;
            var detalles = new List<string>();

            if (request.Hectareas.HasValue)
            {
                var t = request.Hectareas.Value * precioHectarea;
                total += t;
                detalles.Add($"Hectáreas: {request.Hectareas} * {precioHectarea} = {t}");
            }

            if (request.Toneladas.HasValue)
            {
                var t = request.Toneladas.Value * precioTonelada;
                total += t;
                detalles.Add($"Toneladas: {request.Toneladas} * {precioTonelada} = {t}");
            }

            if (request.Km.HasValue && request.Toneladas.HasValue)
            {
                var t = request.Km.Value * request.Toneladas.Value * tarifaKm;
                total += t;
                detalles.Add($"Transporte: {request.Km} km * {request.Toneladas} ton * {tarifaKm} = {t}");
            }

            return Ok(new
            {
                total,
                detalles,
                isMock = true
            });
        }
    }
}
