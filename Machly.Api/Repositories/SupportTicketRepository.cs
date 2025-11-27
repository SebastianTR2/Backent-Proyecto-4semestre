using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class SupportTicketRepository
    {
        private readonly IMongoCollection<SupportTicket> _tickets;

        public SupportTicketRepository(MongoDbContext context)
        {
            _tickets = context.GetCollection<SupportTicket>("support_tickets");
        }

        public async Task CreateAsync(SupportTicket ticket) =>
            await _tickets.InsertOneAsync(ticket);

        public async Task<List<SupportTicket>> GetByUserAsync(string userId) =>
            await _tickets.Find(t => t.UserId == userId).SortByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<List<SupportTicket>> GetAllAsync() =>
            await _tickets.Find(_ => true).SortByDescending(t => t.CreatedAt).ToListAsync();

        public async Task<SupportTicket?> GetByIdAsync(string id) =>
            await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task UpdateAsync(string id, SupportTicket ticket) =>
            await _tickets.ReplaceOneAsync(t => t.Id == id, ticket);
    }
}
