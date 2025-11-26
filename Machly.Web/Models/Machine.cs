using System;
using System.Collections.Generic;

namespace Machly.Web.Models
{
    public class Machine
    {
        public string? Id { get; set; }

        public string ProviderId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";

        // "URBANA" | "AGRICOLA"
        public string Type { get; set; } = "";

        public string Category { get; set; } = "";

        public decimal PricePerDay { get; set; }
        public bool WithOperator { get; set; }

        public double Lat { get; set; }
        public double Lng { get; set; }

        public TariffAgro? TariffsAgro { get; set; }

        public List<MachinePhoto>? Photos { get; set; } = new();

        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class MachinePhoto
    {
        public string Url { get; set; } = "";
        public bool IsCover { get; set; }
    }

    public class TariffAgro
    {
        public decimal? Hectarea { get; set; }
        public decimal? Tonelada { get; set; }

        public List<KmTariff>? KmTariffs { get; set; }
    }

    public class KmTariff
    {
        public int MinKm { get; set; }
        public int MaxKm { get; set; }
        public decimal TarifaPorKm { get; set; }
    }
}
