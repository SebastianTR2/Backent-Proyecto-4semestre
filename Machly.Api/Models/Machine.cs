using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        // Relación con el proveedor (User con Role = PROVIDER)
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProviderId { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string Type { get; set; } = "URBANA";
        public string Category { get; set; } = "GENERAL";

        // Precio base por día (modo estándar)
        public decimal PricePerDay { get; set; }
        public bool WithOperator { get; set; }

        // Geolocalización SIMPLE (como acordamos)
        [BsonElement("Lat")]
        public double Lat { get; set; }

        [BsonElement("Lng")]
        public double Lng { get; set; }
        public LocationPoint? Location { get; set; }

        public List<MachinePhoto> Photos { get; set; } = new();
        public TariffAgro? TariffsAgro { get; set; }

        // Calendario de bloques ocupados (sprint futuro)
        public List<MachineCalendarSlot> Calendar { get; set; } = new();

        // Ratings
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }

        // Auditoría básica
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LocationPoint
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class MachinePhoto
    {
        public string Url { get; set; } = null!;
        public bool IsCover { get; set; }
    }

    public class MachineCalendarSlot
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class TariffAgro
    {
        public decimal? Hectarea { get; set; }
        public decimal? Tonelada { get; set; }
        public List<KmTariff> KmTariffs { get; set; } = new();
    }

    public class KmTariff
    {
        public int MinKm { get; set; }
        public int MaxKm { get; set; }
        public decimal TarifaPorKm { get; set; }
    }
}
