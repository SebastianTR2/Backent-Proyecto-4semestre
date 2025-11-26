using System;
using System.Collections.Generic;

namespace Machly.Web.Models.Admin
{
    public class AdminUserDetailsDto
    {
        public AdminUserSummaryDto User { get; set; } = new();
        public List<AdminMachineSummaryDto> ProviderMachines { get; set; } = new();
        public List<AdminBookingSummaryDto> RenterBookings { get; set; } = new();
    }

    public class AdminUserSummaryDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVerifiedProvider { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminMachineSummaryDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public bool WithOperator { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AdminBookingSummaryDto
    {
        public string? Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string MachineTitle { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }
}

