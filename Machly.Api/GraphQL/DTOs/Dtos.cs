namespace Machly.Api.GraphQL.DTOs
{
    public class MachineDto
    {
        public string Id { get; set; } = null!;
        public string ProviderId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal PricePerDay { get; set; }
        public string Category { get; set; } = null!;
        public string Type { get; set; } = null!;
        public double Lat { get; set; }
        public double Lng { get; set; }
        public List<string> Photos { get; set; } = new();
        public bool IsOutOfService { get; set; }
        public double RatingAvg { get; set; }
        public int RatingCount { get; set; }
    }

    public class BookingDto
    {
        public string Id { get; set; } = null!;
        public string MachineId { get; set; } = null!;
        public string RenterId { get; set; } = null!;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? ReviewRating { get; set; }
        public string? ReviewComment { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? PhotoUrl { get; set; }
    }
}
