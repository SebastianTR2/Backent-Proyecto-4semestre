using System;
using System.Collections.Generic;
using Machly.Api.Models;

namespace Machly.Api.DTOs
{
    public class AdminMachineDetailsDto
    {
        public Machine Machine { get; set; } = null!;
        public AdminUserSummaryDto? Provider { get; set; }
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

