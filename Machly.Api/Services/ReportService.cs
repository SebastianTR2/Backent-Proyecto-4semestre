using Machly.Api.DTOs.Reports;
using Machly.Api.Models;
using Machly.Api.Repositories;
using MongoDB.Driver;

namespace Machly.Api.Services
{
    public class ReportService
    {
        private readonly BookingRepository _bookingRepo;
        private readonly MachineRepository _machineRepo;
        private readonly MongoDbContext _context;

        public ReportService(BookingRepository bookingRepo, MachineRepository machineRepo, MongoDbContext context)
        {
            _bookingRepo = bookingRepo;
            _machineRepo = machineRepo;
            _context = context;
        }

        public async Task<ProviderIncomeReportDto> GetProviderIncomeReport(string providerId, DateTime? from = null, DateTime? to = null)
        {
            // 1. Get Provider Machines
            var machines = await _machineRepo.GetByProviderAsync(providerId);
            var machineIds = machines.Select(m => m.Id).ToList();
            var machineDict = machines.ToDictionary(m => m.Id!);

            // 2. Get Completed Bookings
            var bookingsCollection = _context.GetCollection<Booking>("bookings");
            var filter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.In(b => b.MachineId, machineIds),
                Builders<Booking>.Filter.Eq(b => b.Status, "COMPLETED")
            );

            if (from.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Gte(b => b.End, from.Value));
            if (to.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Lte(b => b.End, to.Value));

            var bookings = await bookingsCollection.Find(filter).ToListAsync();

            // 3. Aggregate
            var report = new ProviderIncomeReportDto();
            report.TotalIncome = bookings.Sum(b => b.TotalPrice);

            // By Machine
            report.Machines = bookings
                .GroupBy(b => b.MachineId)
                .Select(g => new MachineIncomeDto
                {
                    MachineId = g.Key,
                    MachineTitle = machineDict.ContainsKey(g.Key) ? machineDict[g.Key].Title : "Unknown",
                    Income = g.Sum(b => b.TotalPrice)
                })
                .ToList();

            // By Service Type (Method)
            // Note: Booking.Method might be null for standard bookings, infer from context or use default "DIA" if missing
            report.IncomeByServiceType = bookings
                .GroupBy(b => b.Method ?? "DIA") 
                .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice));

            // By Month
            report.IncomeByMonth = bookings
                .GroupBy(b => b.End.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice));

            return report;
        }

        public async Task<ProviderUsageReportDto> GetProviderUsageReport(string providerId, DateTime? from = null, DateTime? to = null)
        {
            var machines = await _machineRepo.GetByProviderAsync(providerId);
            var machineIds = machines.Select(m => m.Id).ToList();
            var machineDict = machines.ToDictionary(m => m.Id!);

            var bookingsCollection = _context.GetCollection<Booking>("bookings");
            // Include CONFIRMED, IN_PROGRESS, COMPLETED for usage stats
            var filter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.In(b => b.MachineId, machineIds),
                Builders<Booking>.Filter.In(b => b.Status, new[] { "CONFIRMED", "IN_PROGRESS", "COMPLETED" })
            );

            if (from.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Gte(b => b.End, from.Value));
            if (to.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Lte(b => b.End, to.Value));

            var bookings = await bookingsCollection.Find(filter).ToListAsync();

            var report = new ProviderUsageReportDto();
            
            // Calculate total days in range for occupancy
            double totalDaysRange = 30; // Default
            if (from.HasValue && to.HasValue)
                totalDaysRange = (to.Value - from.Value).TotalDays;
            if (totalDaysRange < 1) totalDaysRange = 1;

            report.Machines = machines.Select(m =>
            {
                var machineBookings = bookings.Where(b => b.MachineId == m.Id).ToList();
                var totalDays = machineBookings.Sum(b => (b.End - b.Start).TotalDays);
                
                return new MachineUsageDto
                {
                    MachineId = m.Id!,
                    MachineTitle = m.Title,
                    TotalBookings = machineBookings.Count,
                    TotalDaysRented = totalDays,
                    OccupancyPercentage = Math.Min(100, (totalDays / totalDaysRange) * 100),
                    UsageByServiceType = machineBookings
                        .GroupBy(b => b.Method ?? "DIA")
                        .ToDictionary(g => g.Key, g => g.Count())
                };
            }).ToList();

            return report;
        }

        public async Task<ProviderAgroReportDto> GetProviderAgroReport(string providerId, DateTime? from = null, DateTime? to = null)
        {
            var machines = await _machineRepo.GetByProviderAsync(providerId);
            var machineIds = machines.Select(m => m.Id).ToList();
            var machineDict = machines.ToDictionary(m => m.Id!);

            var bookingsCollection = _context.GetCollection<Booking>("bookings");
            var filter = Builders<Booking>.Filter.And(
                Builders<Booking>.Filter.In(b => b.MachineId, machineIds),
                Builders<Booking>.Filter.Eq(b => b.Status, "COMPLETED"),
                Builders<Booking>.Filter.Eq(b => b.Type, "AGRONOMICA") // Only Agro
            );

            if (from.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Gte(b => b.End, from.Value));
            if (to.HasValue)
                filter = Builders<Booking>.Filter.And(filter, Builders<Booking>.Filter.Lte(b => b.End, to.Value));

            var bookings = await bookingsCollection.Find(filter).ToListAsync();

            var report = new ProviderAgroReportDto();

            foreach (var b in bookings)
            {
                if (b.Values != null)
                {
                    report.TotalHectares += b.Values.Hectareas ?? 0;
                    report.TotalTons += b.Values.Toneladas ?? 0;
                    report.TotalKm += b.Values.Km ?? 0;
                }
            }

            report.IncomeByAgroType = bookings
                .GroupBy(b => b.Method ?? "OTRO")
                .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice));

            report.Details = bookings
                .GroupBy(b => b.MachineId)
                .Select(g => new AgroDetailDto
                {
                    MachineTitle = machineDict.ContainsKey(g.Key) ? machineDict[g.Key].Title : "Unknown",
                    Hectares = g.Sum(b => b.Values?.Hectareas ?? 0),
                    Tons = g.Sum(b => b.Values?.Toneladas ?? 0),
                    Km = g.Sum(b => b.Values?.Km ?? 0)
                })
                .ToList();

            return report;
        }
    }
}
