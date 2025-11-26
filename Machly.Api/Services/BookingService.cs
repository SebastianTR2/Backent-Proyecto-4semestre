using Machly.Api.DTOs;
using Machly.Api.Models;
using Machly.Api.Repositories;
using MongoDB.Driver;

namespace Machly.Api.Services
{
    public class BookingService
    {
        private readonly BookingRepository _bookingRepo;
        private readonly MachineRepository _machineRepo;
        private readonly MongoDbContext _context;

        public BookingService(BookingRepository bookingRepo, MachineRepository machineRepo, MongoDbContext context)
        {
            _bookingRepo = bookingRepo;
            _machineRepo = machineRepo;
            _context = context;
        }

        public async Task<Booking?> CreateAsync(BookingCreateRequest request)
        {
            // Validar fechas
            if (request.End <= request.Start)
                return null;

            if (request.Start < DateTime.UtcNow.Date)
                return null;

            var machine = await _machineRepo.GetByIdAsync(request.MachineId);
            if (machine == null) return null;

            // Validar disponibilidad
            var isAvailable = await _bookingRepo.CheckAvailabilityAsync(request.MachineId, request.Start, request.End);
            if (!isAvailable)
                return null;

            var days = (request.End.Date - request.Start.Date).TotalDays;
            if (days < 1) days = 1;

            var booking = new Booking
            {
                MachineId = request.MachineId,
                RenterId = request.RenterId,
                Start = request.Start,
                End = request.End,
                TotalPrice = machine.PricePerDay * (decimal)days,
                Status = "CONFIRMED",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepo.CreateAsync(booking);
            return booking;
        }

        public Task<List<Booking>> GetByUserAsync(string renterId, DateTime? from = null, DateTime? to = null) =>
            _bookingRepo.GetByUserAsync(renterId, from, to);

        public Task<List<Booking>> GetByMachineAsync(string machineId, DateTime? from = null, DateTime? to = null) =>
            _bookingRepo.GetByMachineAsync(machineId, from, to);

        public async Task<List<Booking>> GetByProviderAsync(string providerId, DateTime? from = null, DateTime? to = null)
        {
            var machinesCollection = _context.GetCollection<Machine>("machines");
            return await _bookingRepo.GetByProviderAsync(providerId, machinesCollection, from, to);
        }

        public Task<Booking?> GetByIdAsync(string id) =>
            _bookingRepo.GetByIdAsync(id);

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            booking.Status = status;
            booking.UpdatedAt = DateTime.UtcNow;
            return await _bookingRepo.UpdateAsync(booking);
        }

        public async Task<bool> CheckInAsync(string id, List<string> photos)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            booking.CheckInDate = DateTime.UtcNow;
            booking.CheckInPhotos = photos;
            booking.Status = "IN_PROGRESS";
            booking.UpdatedAt = DateTime.UtcNow;
            return await _bookingRepo.UpdateAsync(booking);
        }

        public async Task<bool> CheckOutAsync(string id, List<string> photos)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            booking.CheckOutDate = DateTime.UtcNow;
            booking.CheckOutPhotos = photos;
            booking.Status = "COMPLETED";
            booking.UpdatedAt = DateTime.UtcNow;
            return await _bookingRepo.UpdateAsync(booking);
        }

        public async Task<bool> AddReviewAsync(string id, int rating, string comment)
        {
            if (rating < 1 || rating > 5) return false;

            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null) return false;

            booking.Review = new BookingReview
            {
                Rating = rating,
                Comment = comment
            };

            // Actualizar rating de la máquina
            var machine = await _machineRepo.GetByIdAsync(booking.MachineId);
            if (machine != null)
            {
                var totalRating = machine.RatingAvg * machine.RatingCount + rating;
                machine.RatingCount++;
                machine.RatingAvg = totalRating / machine.RatingCount;
                await _machineRepo.UpdateAsync(machine.Id!, machine);
            }

            booking.UpdatedAt = DateTime.UtcNow;
            return await _bookingRepo.UpdateAsync(booking);
        }

        public Task<List<Booking>> GetAllAsync(DateTime? from = null, DateTime? to = null) =>
            _bookingRepo.GetAllAsync(from, to);
    }
}
