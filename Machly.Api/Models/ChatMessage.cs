using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string SenderId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReceiverId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        public string? BookingId { get; set; } // Opcional, para contexto

        public string Message { get; set; } = null!;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
