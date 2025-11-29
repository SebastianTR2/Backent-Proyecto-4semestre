using System;
using System.Collections.Generic;

namespace Machly.Web.Models.Admin
{
    public class AdminUserDetailsDto
    {
        public ApiAdminUserSummaryDto User { get; set; } = new();
        public List<AdminMachineListItem> ProviderMachines { get; set; } = new();
        public List<AdminBookingListItem> RenterBookings { get; set; } = new();
    }
}

