using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } = ""; // BOOKING_CREATED, BOOKING_CONFIRMED, etc.

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

