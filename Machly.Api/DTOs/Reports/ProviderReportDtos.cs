namespace Machly.Api.DTOs.Reports
{
    public class ProviderIncomeReportDto
    {
        public decimal TotalIncome { get; set; }
        public List<MachineIncomeDto> Machines { get; set; } = new();
        public Dictionary<string, decimal> IncomeByServiceType { get; set; } = new();
        public Dictionary<string, decimal> IncomeByMonth { get; set; } = new();
    }

    public class MachineIncomeDto
    {
        public string MachineId { get; set; } = "";
        public string MachineTitle { get; set; } = "";
        public decimal Income { get; set; }
    }

    public class ProviderUsageReportDto
    {
        public List<MachineUsageDto> Machines { get; set; } = new();
    }

    public class MachineUsageDto
    {
        public string MachineId { get; set; } = "";
        public string MachineTitle { get; set; } = "";
        public int TotalBookings { get; set; }
        public double TotalDaysRented { get; set; }
        public double OccupancyPercentage { get; set; } // Simple: days rented / days in range
        public Dictionary<string, int> UsageByServiceType { get; set; } = new();
    }

    public class ProviderAgroReportDto
    {
        public double TotalHectares { get; set; }
        public double TotalTons { get; set; }
        public double TotalKm { get; set; }
        public Dictionary<string, decimal> IncomeByAgroType { get; set; } = new();
        public List<AgroDetailDto> Details { get; set; } = new();
    }

    public class AgroDetailDto
    {
        public string MachineTitle { get; set; } = "";
        public double Hectares { get; set; }
        public double Tons { get; set; }
        public double Km { get; set; }
    }
}
