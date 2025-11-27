using System;
using System.Collections.Generic;
using Machly.Api.Models;

namespace Machly.Api.DTOs
{
    public class AdminUserDetailsDto
    {
        public ApiAdminUserSummaryDto User { get; set; } = null!;
        public List<AdminMachineListItem> ProviderMachines { get; set; } = new();
        public List<AdminBookingListItem> RenterBookings { get; set; } = new();
    }
}

