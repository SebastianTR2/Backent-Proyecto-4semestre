namespace Machly.Api.DTOs
{
    public class BookingDetailDto
    {
        public string Id { get; set; } = null!;
        public string MachineId { get; set; } = null!;
        public string MachineTitle { get; set; } = null!;
        public string? MachinePhotoUrl { get; set; }
        
        public string RenterId { get; set; } = null!;
        public string RenterName { get; set; } = null!;
        public string? RenterEmail { get; set; }
        public string? RenterPhotoUrl { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "PENDING";
        
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
    }
}
