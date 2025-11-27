using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }

        public string Action { get; set; } = null!; // LOGIN, CREATE_BOOKING, etc.
        public string EntityType { get; set; } = null!; // Machine, Booking, User
        public string? EntityId { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Metadata { get; set; } // JSON extra info
    }
}
