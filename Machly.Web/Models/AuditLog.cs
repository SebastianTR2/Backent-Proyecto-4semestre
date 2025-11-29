using System;

namespace Machly.Web.Models
{
    public class AuditLog
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string? EntityId { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Metadata { get; set; }
    }
}
