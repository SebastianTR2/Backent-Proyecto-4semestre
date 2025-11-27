using Machly.Api.Models;
using Machly.Api.Repositories;

namespace Machly.Api.Services
{
    public class AuditService
    {
        private readonly AuditRepository _repo;

        public AuditService(AuditRepository repo)
        {
            _repo = repo;
        }

        public async Task LogAsync(string? userId, string action, string entityType, string? entityId, string? metadata = null)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Metadata = metadata,
                Timestamp = DateTime.UtcNow
            };
            await _repo.LogAsync(log);
        }

        public Task<List<AuditLog>> GetLogsAsync(DateTime? from, DateTime? to, string? userId, string? action) =>
            _repo.GetLogsAsync(from, to, userId, action);
    }
}
