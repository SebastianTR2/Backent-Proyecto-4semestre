namespace Machly.Web.Models
{
    public class Booking
    {
        public string? Id { get; set; }
        public string MachineId { get; set; } = "";
        public string RenterId { get; set; } = "";
        public string? ProviderId { get; set; }
        public string Type { get; set; } = "ESTANDAR";
        public string? Method { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Deposit { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "PENDING";
        public DateTime? CheckInDate { get; set; }
        public List<string> CheckInPhotos { get; set; } = new();
        public DateTime? CheckOutDate { get; set; }
        public List<string> CheckOutPhotos { get; set; } = new();
        public BookingReview? Review { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BookingReview
    {
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
    }
}

