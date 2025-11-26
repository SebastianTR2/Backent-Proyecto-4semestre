using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class NotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationRepository(MongoDbContext context)
        {
            _notifications = context.GetCollection<Notification>("notifications");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var userIdIndex = Builders<Notification>.IndexKeys.Ascending(n => n.UserId);
                _notifications.Indexes.CreateOne(new CreateIndexModel<Notification>(userIdIndex));

                var readIndex = Builders<Notification>.IndexKeys.Ascending(n => n.IsRead);
                _notifications.Indexes.CreateOne(new CreateIndexModel<Notification>(readIndex));
            }
            catch { }
        }

        public async Task CreateAsync(Notification notification) =>
            await _notifications.InsertOneAsync(notification);

        public async Task<List<Notification>> GetByUserAsync(string userId) =>
            await _notifications.Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<bool> MarkAsReadAsync(string id)
        {
            var filter = Builders<Notification>.Filter.Eq("_id", MongoDB.Bson.ObjectId.Parse(id));
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            var result = await _notifications.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}

