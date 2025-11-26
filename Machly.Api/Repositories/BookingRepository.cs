using Machly.Api.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Machly.Api.Repositories
{
    public class BookingRepository
    {
        private readonly IMongoCollection<Booking> _bookings;

        public BookingRepository(MongoDbContext context)
        {
            _bookings = context.GetCollection<Booking>("bookings");
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                // Índice en RenterId
                var renterIndex = Builders<Booking>.IndexKeys.Ascending(b => b.RenterId);
                _bookings.Indexes.CreateOne(new CreateIndexModel<Booking>(renterIndex));

                // Índice en MachineId
                var machineIndex = Builders<Booking>.IndexKeys.Ascending(b => b.MachineId);
                _bookings.Indexes.CreateOne(new CreateIndexModel<Booking>(machineIndex));

                // Índice compuesto para validar disponibilidad
                var availabilityIndex = Builders<Booking>.IndexKeys
                    .Ascending(b => b.MachineId)
                    .Ascending(b => b.Start)
                    .Ascending(b => b.End);
                _bookings.Indexes.CreateOne(new CreateIndexModel<Booking>(availabilityIndex));

                // Índice en Status
                var statusIndex = Builders<Booking>.IndexKeys.Ascending(b => b.Status);
                _bookings.Indexes.CreateOne(new CreateIndexModel<Booking>(statusIndex));
            }
            catch { } // Ignorar si ya existen
        }

        public async Task CreateAsync(Booking booking) =>
            await _bookings.InsertOneAsync(booking);

        public async Task<List<Booking>> GetByUserAsync(string renterId, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<Booking>.Filter.Eq(b => b.RenterId, renterId);
            filter = ApplyDateRange(filter, from, to);
            return await _bookings.Find(filter)
                .SortByDescending(b => b.Start)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByMachineAsync(string machineId, DateTime? from = null, DateTime? to = null)
        {
            var filter = Builders<Booking>.Filter.Eq(b => b.MachineId, machineId);
            filter = ApplyDateRange(filter, from, to);
            return await _bookings.Find(filter)
                .SortByDescending(b => b.Start)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetByProviderAsync(
            string providerId,
            IMongoCollection<Machine> machinesCollection,
            DateTime? from = null,
            DateTime? to = null)
        {
            // Obtener IDs de máquinas del proveedor
            var machineIds = await machinesCollection
                .Find(m => m.ProviderId == providerId)
                .Project(m => m.Id)
                .ToListAsync();

            var filter = Builders<Booking>.Filter.In(b => b.MachineId, machineIds);
            filter = ApplyDateRange(filter, from, to);
            return await _bookings.Find(filter)
                .SortByDescending(b => b.Start)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            var filter = Builders<Booking>.Filter.Eq("_id", objectId);
            return await _bookings.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckAvailabilityAsync(string machineId, DateTime start, DateTime end, string? excludeBookingId = null)
        {
            var filters = new List<FilterDefinition<Booking>>
            {
                Builders<Booking>.Filter.Eq(b => b.MachineId, machineId),
                Builders<Booking>.Filter.Eq(b => b.Status, "CONFIRMED"),
                Builders<Booking>.Filter.Or(
                    // Solapamiento: start dentro del rango existente
                    Builders<Booking>.Filter.And(
                        Builders<Booking>.Filter.Lte(b => b.Start, start),
                        Builders<Booking>.Filter.Gte(b => b.End, start)
                    ),
                    // Solapamiento: end dentro del rango existente
                    Builders<Booking>.Filter.And(
                        Builders<Booking>.Filter.Lte(b => b.Start, end),
                        Builders<Booking>.Filter.Gte(b => b.End, end)
                    ),
                    // Solapamiento: rango existente dentro del nuevo rango
                    Builders<Booking>.Filter.And(
                        Builders<Booking>.Filter.Gte(b => b.Start, start),
                        Builders<Booking>.Filter.Lte(b => b.End, end)
                    )
                )
            };

            if (!string.IsNullOrEmpty(excludeBookingId))
            {
                var excludeObjectId = ObjectId.Parse(excludeBookingId);
                filters.Add(Builders<Booking>.Filter.Ne("_id", excludeObjectId));
            }

            var filter = Builders<Booking>.Filter.And(filters);
            var existing = await _bookings.Find(filter).FirstOrDefaultAsync();
            return existing == null; // true si está disponible
        }

        public async Task<bool> UpdateAsync(Booking booking)
        {
            var filter = Builders<Booking>.Filter.Eq("_id", ObjectId.Parse(booking.Id!));
            booking.UpdatedAt = DateTime.UtcNow;
            var result = await _bookings.ReplaceOneAsync(filter, booking);
            return result.ModifiedCount > 0;
        }

        public async Task<List<Booking>> GetAllAsync(DateTime? from = null, DateTime? to = null)
        {
            var filter = ApplyDateRange(Builders<Booking>.Filter.Empty, from, to);
            return await _bookings.Find(filter)
                .SortByDescending(b => b.Start)
                .ToListAsync();
        }

        private FilterDefinition<Booking> ApplyDateRange(FilterDefinition<Booking> filter, DateTime? from, DateTime? to)
        {
            var result = filter;
            if (from.HasValue)
            {
                var fromUtc = DateTime.SpecifyKind(from.Value, DateTimeKind.Utc);
                result = Builders<Booking>.Filter.And(result, Builders<Booking>.Filter.Gte(b => b.Start, fromUtc));
            }
            if (to.HasValue)
            {
                var toUtc = DateTime.SpecifyKind(to.Value, DateTimeKind.Utc);
                result = Builders<Booking>.Filter.And(result, Builders<Booking>.Filter.Lte(b => b.End, toUtc));
            }
            return result;
        }
    }
}
