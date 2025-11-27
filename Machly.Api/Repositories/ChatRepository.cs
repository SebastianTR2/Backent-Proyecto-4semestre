using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class ChatRepository
    {
        private readonly IMongoCollection<ChatMessage> _messages;

        public ChatRepository(MongoDbContext context)
        {
            _messages = context.GetCollection<ChatMessage>("chat_messages");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var keys = Builders<ChatMessage>.IndexKeys;
                _messages.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(keys.Ascending(m => m.SenderId)));
                _messages.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(keys.Ascending(m => m.ReceiverId)));
                _messages.Indexes.CreateOne(new CreateIndexModel<ChatMessage>(keys.Ascending(m => m.BookingId)));
            }
            catch { }
        }

        public async Task CreateAsync(ChatMessage message) =>
            await _messages.InsertOneAsync(message);

        public async Task<List<ChatMessage>> GetConversationAsync(string userId1, string userId2)
        {
            var filter = Builders<ChatMessage>.Filter.Or(
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.SenderId, userId1),
                    Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, userId2)
                ),
                Builders<ChatMessage>.Filter.And(
                    Builders<ChatMessage>.Filter.Eq(m => m.SenderId, userId2),
                    Builders<ChatMessage>.Filter.Eq(m => m.ReceiverId, userId1)
                )
            );

            return await _messages.Find(filter)
                .SortBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetByBookingAsync(string bookingId)
        {
            return await _messages.Find(m => m.BookingId == bookingId)
                .SortBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
