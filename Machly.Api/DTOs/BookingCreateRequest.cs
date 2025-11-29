namespace Machly.Api.DTOs
{
    public class BookingCreateRequest
    {
        public string MachineId { get; set; } = null!;
        public string RenterId { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Opcional, por si en algún lado lo usan al mapear
        public DateTime? CreatedAt { get; set; }

        // Datos agronómicos opcionales
        public double? Hectareas { get; set; }
        public double? Toneladas { get; set; }
        public double? Km { get; set; }

        public string? ServiceType { get; set; } // DIA | HORA (RF-07)
    }
}
