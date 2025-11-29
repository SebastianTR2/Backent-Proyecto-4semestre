using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Machly.Api.Models
{
    public class Machine
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProviderId { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Type { get; set; } = "URBANA"; // e.g., URBANA, AGRONOMICA
        public string Category { get; set; } = "GENERAL";

        public decimal PricePerDay { get; set; }
        public decimal PricePerHour { get; set; } // new for hourly bookings
        public bool WithOperator { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }
        public LocationPoint? Location { get; set; }

        [BsonIgnoreIfNull]
        public MongoDB.Driver.GeoJsonObjectModel.GeoJsonPoint<MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates>? GeoLocation { get; set; }

        public List<MachinePhoto> Photos { get; set; } = new();
        public TariffAgro? TariffsAgro { get; set; }
        public List<DateRange> Calendar { get; set; } = new();
        public bool IsOutOfService { get; set; } = false;
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
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

    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Type { get; set; } = "BLOCKED"; // BLOCKED, MAINTENANCE
    }

    public class TariffAgro
    {
        public decimal? Hectarea { get; set; }
        public decimal? Tonelada { get; set; }
        public decimal? Km { get; set; } // simple per‑km price
        public List<KmTariff> KmTariffs { get; set; } = new();
    }

    public class KmTariff
    {
        public int MinKm { get; set; }
        public int MaxKm { get; set; }
        public decimal TarifaPorKm { get; set; }
    }
}
