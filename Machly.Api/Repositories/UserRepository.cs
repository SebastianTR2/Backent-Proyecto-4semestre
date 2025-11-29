using Machly.Api.Models;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(MongoDbContext context)
        {
            _users = context.GetCollection<User>("users");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var keys = Builders<User>.IndexKeys;
                var emailIndex = new CreateIndexModel<User>(keys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true });
                _users.Indexes.CreateOne(emailIndex);
                _users.Indexes.CreateOne(new CreateIndexModel<User>(keys.Ascending(u => u.Role)));
            }
            catch { }
        }

        // Obtener por email
        public async Task<User?> GetByEmailAsync(string email) =>
            await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

        // Obtener por ID
        public async Task<User?> GetByIdAsync(string id) =>
            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        // Crear usuario
        public async Task CreateAsync(User user) =>
            await _users.InsertOneAsync(user);

        public async Task<bool> UpdateAsync(User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
            var result = await _users.ReplaceOneAsync(filter, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _users.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        // 🔥 NECESARIO PARA EL SEED
        public async Task<List<User>> GetAllAsync() =>
            await _users.Find(_ => true).ToListAsync();

        // Obtener por Rol
        public async Task<List<User>> GetByRoleAsync(string role) =>
            await _users.Find(u => u.Role == role).ToListAsync();
    }
}
