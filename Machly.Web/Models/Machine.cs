using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Machly.Web.Models
{
    public class Machine
    {
        public string? Id { get; set; }

        public string ProviderId { get; set; } = "";

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Type { get; set; } = "";

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public string Category { get; set; } = "";

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PricePerDay { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal PricePerHour { get; set; } // RF-07

        public bool WithOperator { get; set; }
        public List<MachinePhoto> Photos { get; set; } = new();

        [Range(-90, 90, ErrorMessage = "Latitud inválida (-90 a 90)")]
        public double Lat { get; set; }

        [Range(-180, 180, ErrorMessage = "Longitud inválida (-180 a 180)")]
        public double Lng { get; set; }

        public TariffAgro? TariffsAgro { get; set; }
        public bool IsOutOfService { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
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
        public decimal? Km { get; set; } // RF-08
        public List<KmTariff>? KmTariffs { get; set; } = new();
    }

    public class KmTariff
    {
        public int MinKm { get; set; }
        public int MaxKm { get; set; }
        public decimal TarifaPorKm { get; set; }
    }
}