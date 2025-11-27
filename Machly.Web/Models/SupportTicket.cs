namespace Machly.Web.Models
{
    public class SupportTicket
    {
        public string? Id { get; set; }
        public string UserId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; } = "OPEN";
        public string Priority { get; set; } = "MEDIUM";
        public string? AdminResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public class CreateTicketViewModel
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; } = "MEDIUM";
    }
}
