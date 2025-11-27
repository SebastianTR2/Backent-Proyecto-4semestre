using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class AuditRepository
    {
        private readonly IMongoCollection<AuditLog> _logs;

        public AuditRepository(MongoDbContext context)
        {
            _logs = context.GetCollection<AuditLog>("audit_logs");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var keys = Builders<AuditLog>.IndexKeys;
                _logs.Indexes.CreateOne(new CreateIndexModel<AuditLog>(keys.Ascending(l => l.UserId)));
                _logs.Indexes.CreateOne(new CreateIndexModel<AuditLog>(keys.Descending(l => l.Timestamp)));
                _logs.Indexes.CreateOne(new CreateIndexModel<AuditLog>(keys.Ascending(l => l.Action)));
            }
            catch { }
        }

        public async Task LogAsync(AuditLog log) =>
            await _logs.InsertOneAsync(log);

        public async Task<List<AuditLog>> GetLogsAsync(DateTime? from, DateTime? to, string? userId, string? action)
        {
            var builder = Builders<AuditLog>.Filter;
            var filters = new List<FilterDefinition<AuditLog>>();

            if (from.HasValue) filters.Add(builder.Gte(l => l.Timestamp, from.Value));
            if (to.HasValue) filters.Add(builder.Lte(l => l.Timestamp, to.Value));
            if (!string.IsNullOrEmpty(userId)) filters.Add(builder.Eq(l => l.UserId, userId));
            if (!string.IsNullOrEmpty(action)) filters.Add(builder.Eq(l => l.Action, action));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _logs.Find(finalFilter)
                .SortByDescending(l => l.Timestamp)
                .Limit(100)
                .ToListAsync();
        }
    }
}
