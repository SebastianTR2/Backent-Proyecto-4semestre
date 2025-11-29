using System;
using System.Collections.Generic;

namespace Machly.Web.Models.Admin
{
    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Info";
        public DateTime Timestamp { get; set; }
    }

    public class IncomeReportDto
    {
        public decimal TotalIncome { get; set; }
        public List<IncomeByMachine> Machines { get; set; } = new();
        // Added to support Monthly chart if needed
        public List<IncomeByMonth> Monthly { get; set; } = new();
    }

    public class IncomeByMachine
    {
        public string MachineName { get; set; } = string.Empty;
        public decimal Income { get; set; }
    }

    public class IncomeByMonth
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
    }

    public class OccupancyReportDto
    {
        public List<MachineOccupancy> Items { get; set; } = new();
    }

    public class MachineOccupancy
    {
        public string MachineName { get; set; } = string.Empty;
        public int OccupancyPercentage { get; set; } // We will map Count here for now
        public int BookingCount { get; set; }
    }
}
