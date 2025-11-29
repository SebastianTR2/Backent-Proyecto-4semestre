using System;
using System.ComponentModel.DataAnnotations;

namespace Machly.Web.Models.ViewModels
{
    public class CreateBookingViewModel
    {
        public string? RenterId { get; set; } // Added for explicit passing

        [Required]
        public string MachineId { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public decimal? Hectareas { get; set; }
        public decimal? Toneladas { get; set; }
        public decimal? Kilometros { get; set; }

        public string? ServiceType { get; set; } // DIA | HORA (RF-07)
    }
}
