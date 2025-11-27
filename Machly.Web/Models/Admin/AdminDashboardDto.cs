namespace Machly.Web.Models.Admin
{
    public class AdminDashboardDto
    {
        public long TotalUsers { get; set; }
        public long TotalProviders { get; set; }
        public long TotalRenters { get; set; }
        public long TotalMachines { get; set; }
        public long TotalBookings { get; set; }
        public long BookingsLast30Days { get; set; }
        
        public List<CategoryStat> TopCategories { get; set; } = new();
        public List<MachineStat> TopMachines { get; set; } = new();
    }

    public class CategoryStat
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class MachineStat
    {
        public string MachineTitle { get; set; }
        public int BookingsCount { get; set; }
    }
}
