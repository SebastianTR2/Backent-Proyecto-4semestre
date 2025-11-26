using Machly.Api.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Machly.Api.Repositories
{
    public class MachineRepository
    {
        private readonly IMongoCollection<Machine> _machines;

        public MachineRepository(MongoDbContext context)
        {
            _machines = context.GetCollection<Machine>("machines");
            // Crear índices si no existen
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                // Índice en ProviderId
                var providerIndex = Builders<Machine>.IndexKeys.Ascending(m => m.ProviderId);
                _machines.Indexes.CreateOne(new CreateIndexModel<Machine>(providerIndex));
            }
            catch { } // Ignorar si ya existen
        }

        public async Task<List<Machine>> GetAllAsync() =>
            await _machines.Find(_ => true).ToListAsync();

        public async Task<Machine?> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<Machine>.Filter.Eq("_id", objectId);
            return await _machines.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Machine>> GetByProviderAsync(string providerId) =>
            await _machines.Find(m => m.ProviderId == providerId).ToListAsync();

        public async Task<List<Machine>> GetFilteredAsync(
            double? lat = null,
            double? lng = null,
            double? radiusKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? type = null,
            string? category = null,
            bool? withOperator = null,
            string? providerId = null)
        {
            var filters = new List<FilterDefinition<Machine>>();

            // Filtro por precio
            if (minPrice.HasValue || maxPrice.HasValue)
            {
                if (minPrice.HasValue)
                {
                    filters.Add(Builders<Machine>.Filter.Gte(m => m.PricePerDay, minPrice.Value));
                }
                if (maxPrice.HasValue)
                {
                    filters.Add(Builders<Machine>.Filter.Lte(m => m.PricePerDay, maxPrice.Value));
                }
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                filters.Add(Builders<Machine>.Filter.Eq(m => m.Type, type));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                filters.Add(Builders<Machine>.Filter.Eq(m => m.Category, category));
            }

            if (withOperator.HasValue)
            {
                filters.Add(Builders<Machine>.Filter.Eq(m => m.WithOperator, withOperator.Value));
            }

            if (!string.IsNullOrWhiteSpace(providerId))
            {
                filters.Add(Builders<Machine>.Filter.Eq(m => m.ProviderId, providerId));
            }

            var finalFilter = filters.Count > 0
                ? Builders<Machine>.Filter.And(filters)
                : Builders<Machine>.Filter.Empty;

            var machines = await _machines.Find(finalFilter).ToListAsync();

            // Filtro geoespacial simple (en memoria) si se proporciona
            if (lat.HasValue && lng.HasValue && radiusKm.HasValue)
            {
                machines = machines.Where(m =>
                {
                    var distance = CalculateDistance(lat.Value, lng.Value, m.Lat, m.Lng);
                    return distance <= radiusKm.Value;
                }).ToList();
            }

            return machines;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Radio de la Tierra en km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees) => degrees * Math.PI / 180;

        public async Task CreateAsync(Machine machine) =>
            await _machines.InsertOneAsync(machine);

        public async Task<bool> UpdateAsync(string id, Machine machine)
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<Machine>.Filter.Eq("_id", objectId);
            var result = await _machines.ReplaceOneAsync(filter, machine);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<Machine>.Filter.Eq("_id", objectId);
            var result = await _machines.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}
