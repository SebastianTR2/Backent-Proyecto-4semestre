namespace Machly.Api.DTOs
{
    public class NotificationRequest
    {
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = "";
    }
}

