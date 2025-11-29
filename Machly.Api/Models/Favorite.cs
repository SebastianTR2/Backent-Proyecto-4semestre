using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Machly.Api.Models
{
    public class Favorite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string MachineId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
