using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class SupportTicket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string? UserId { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }
        
        // OPEN, IN_PROGRESS, RESOLVED, CLOSED
        public string Status { get; set; } = "OPEN";
        
        // Prioridad: LOW, MEDIUM, HIGH
        public string Priority { get; set; } = "MEDIUM";

        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }
}
