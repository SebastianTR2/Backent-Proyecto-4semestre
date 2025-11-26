using Machly.Api.Models;
using Machly.Api.Repositories;

namespace Machly.Api.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repo;

        public NotificationService(NotificationRepository repo)
        {
            _repo = repo;
        }

        public Task CreateAsync(Notification notification) =>
            _repo.CreateAsync(notification);

        public Task<List<Notification>> GetByUserAsync(string userId) =>
            _repo.GetByUserAsync(userId);

        public Task<bool> MarkAsReadAsync(string id) =>
            _repo.MarkAsReadAsync(id);

        public async Task NotifyBookingCreated(string renterId, string machineTitle)
        {
            var notification = new Notification
            {
                UserId = renterId,
                Title = "Reserva Confirmada",
                Message = $"Tu reserva de {machineTitle} ha sido confirmada.",
                Type = "BOOKING_CREATED"
            };
            await _repo.CreateAsync(notification);
        }

        public async Task NotifyBookingCompleted(string renterId, string machineTitle)
        {
            var notification = new Notification
            {
                UserId = renterId,
                Title = "Reserva Completada",
                Message = $"Tu reserva de {machineTitle} ha sido completada.",
                Type = "BOOKING_COMPLETED"
            };
            await _repo.CreateAsync(notification);
        }
    }
}

