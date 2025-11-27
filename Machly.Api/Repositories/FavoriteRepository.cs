using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class FavoriteRepository
    {
        private readonly IMongoCollection<Favorite> _favorites;

        public FavoriteRepository(MongoDbContext context)
        {
            _favorites = context.GetCollection<Favorite>("favorites");
        }

        public async Task CreateAsync(Favorite favorite)
        {
            // Evitar duplicados
            var exists = await _favorites.Find(f => f.UserId == favorite.UserId && f.MachineId == favorite.MachineId).AnyAsync();
            if (!exists)
            {
                await _favorites.InsertOneAsync(favorite);
            }
        }

        public async Task DeleteAsync(string userId, string machineId) =>
            await _favorites.DeleteOneAsync(f => f.UserId == userId && f.MachineId == machineId);

        public async Task<List<Favorite>> GetByUserAsync(string userId) =>
            await _favorites.Find(f => f.UserId == userId).ToListAsync();
        
        public async Task<bool> IsFavoriteAsync(string userId, string machineId) =>
            await _favorites.Find(f => f.UserId == userId && f.MachineId == machineId).AnyAsync();
    }
}
