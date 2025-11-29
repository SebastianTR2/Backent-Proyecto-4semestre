namespace Machly.Api.DTOs
{
    public class NotificationRequest
    {
        public string Target { get; set; } = "USER"; // ALL, ROLE, USER
        public string? Role { get; set; } // ADMIN, PROVIDER, RENTER
        public string? UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "";
    }
}

