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
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                var indexKeys = Builders<Machine>.IndexKeys;
                
                // Índice en ProviderId
                var providerIndex = new CreateIndexModel<Machine>(indexKeys.Ascending(m => m.ProviderId));
                _machines.Indexes.CreateOne(providerIndex);

                // Índice Geoespacial 2dsphere
                var geoIndex = new CreateIndexModel<Machine>(indexKeys.Geo2DSphere(m => m.GeoLocation));
                _machines.Indexes.CreateOne(geoIndex);
            }
            catch { } 
        }

        public async Task<List<Machine>> GetAllAsync() =>
            await _machines.Find(_ => true).ToListAsync();

        public async Task<Machine?> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<Machine>.Filter.Eq("_id", objectId);
            return await _machines.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<Machine>> GetByIdsAsync(List<string> ids)
        {
            if (ids == null || !ids.Any()) return new List<Machine>();
            
            var objectIds = ids.Select(id => ObjectId.Parse(id)).ToList();
            var filter = Builders<Machine>.Filter.In("_id", objectIds);
            return await _machines.Find(filter).ToListAsync();
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
            var builder = Builders<Machine>.Filter;
            var filters = new List<FilterDefinition<Machine>>();

            // Filtro Geoespacial ($near)
            if (lat.HasValue && lng.HasValue && radiusKm.HasValue)
            {
                var point = new MongoDB.Driver.GeoJsonObjectModel.GeoJsonPoint<MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates>(
                    new MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates(lng.Value, lat.Value));
                
                // $near espera metros, radiusKm * 1000
                filters.Add(builder.Near(m => m.GeoLocation, point, radiusKm.Value * 1000));
            }

            if (minPrice.HasValue) filters.Add(builder.Gte(m => m.PricePerDay, minPrice.Value));
            if (maxPrice.HasValue) filters.Add(builder.Lte(m => m.PricePerDay, maxPrice.Value));
            if (!string.IsNullOrWhiteSpace(type)) filters.Add(builder.Eq(m => m.Type, type));
            if (!string.IsNullOrWhiteSpace(category)) filters.Add(builder.Eq(m => m.Category, category));
            if (withOperator.HasValue) filters.Add(builder.Eq(m => m.WithOperator, withOperator.Value));
            if (!string.IsNullOrWhiteSpace(providerId)) filters.Add(builder.Eq(m => m.ProviderId, providerId));

            var finalFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

            return await _machines.Find(finalFilter).ToListAsync();
        }

        public async Task CreateAsync(Machine machine)
        {
            // Sincronizar GeoLocation
            if (machine.Lat != 0 && machine.Lng != 0)
            {
                machine.GeoLocation = new MongoDB.Driver.GeoJsonObjectModel.GeoJsonPoint<MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates>(
                    new MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates(machine.Lng, machine.Lat));
            }
            await _machines.InsertOneAsync(machine);
        }

        public async Task<bool> UpdateAsync(string id, Machine machine)
        {
            // Sincronizar GeoLocation
            if (machine.Lat != 0 && machine.Lng != 0)
            {
                machine.GeoLocation = new MongoDB.Driver.GeoJsonObjectModel.GeoJsonPoint<MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates>(
                    new MongoDB.Driver.GeoJsonObjectModel.GeoJson2DGeographicCoordinates(machine.Lng, machine.Lat));
            }

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
