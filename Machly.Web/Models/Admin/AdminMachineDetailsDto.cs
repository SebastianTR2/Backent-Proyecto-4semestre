using System;
using System.Collections.Generic;

namespace Machly.Web.Models.Admin
{
    public class AdminMachineDetailsDto
    {
        public Machine Machine { get; set; } = new();
        public ApiAdminUserSummaryDto? Provider { get; set; }
        public List<AdminBookingReviewDto> Reviews { get; set; } = new();
    }

    public class AdminBookingReviewDto
    {
        public string? BookingId { get; set; }
        public string? RenterId { get; set; }
        public string? RenterName { get; set; }
        public BookingReview? Review { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

