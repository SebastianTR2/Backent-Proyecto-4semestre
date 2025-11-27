using System;

namespace Machly.Web.Models.Admin
{
    public class AdminBookingListItem
    {
        public string? Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string MachineTitle { get; set; } = string.Empty;
        public string RenterId { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
    }
}
