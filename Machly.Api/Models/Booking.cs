using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class Booking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string MachineId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string RenterId { get; set; } = null!;

        /// <summary>
        /// Opcional: proveedor dueño de la máquina, útil para queries.
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ProviderId { get; set; }

        /// <summary>
        /// ESTANDAR | AGRONOMICA
        /// </summary>
        public string Type { get; set; } = "ESTANDAR";

        /// <summary>
        /// HECTAREAS | TONELADAS | DISTANCIA (solo si Type = AGRONOMICA)
        /// </summary>
        public string? Method { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        /// <summary>
        /// Valores usados en reservas agronómicas (pueden ser null).
        /// </summary>
        public BookingValues? Values { get; set; }

        public decimal Deposit { get; set; }
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// PENDING | CONFIRMED | COMPLETED | CANCELLED
        /// </summary>
        public string Status { get; set; } = "PENDING";

        // Check-in / Check-out (sprint 2)
        public DateTime? CheckInDate { get; set; }
        public List<string> CheckInPhotos { get; set; } = new();

        public DateTime? CheckOutDate { get; set; }
        public List<string> CheckOutPhotos { get; set; } = new();

        // Review (rating + comentario)
        public BookingReview? Review { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BookingValues
    {
        public double? Hectareas { get; set; }
        public double? Toneladas { get; set; }
        public double? Km { get; set; }
    }

    public class BookingReview
    {
        public int Rating { get; set; }          // 1–5
        public string Comment { get; set; } = "";
    }
}
