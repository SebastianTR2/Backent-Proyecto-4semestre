using Machly.Api.Models;
using Machly.Api.Repositories;

namespace Machly.Api.Services
{
    public class NotificationService
    {
        private readonly NotificationRepository _repo;
        private readonly UserRepository _userRepo;

        public NotificationService(NotificationRepository repo, UserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
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

        public async Task NotifyProviderNewBooking(string providerId, string machineTitle)
        {
            var notification = new Notification
            {
                UserId = providerId,
                Title = "Nueva Reserva",
                Message = $"Has recibido una nueva reserva para tu máquina: {machineTitle}.",
                Type = "BOOKING_NEW"
            };
            await _repo.CreateAsync(notification);
        }

        public async Task NotifyRenterBookingInProgress(string renterId, string machineTitle)
        {
            var notification = new Notification
            {
                UserId = renterId,
                Title = "Reserva en Curso",
                Message = $"El proveedor ha iniciado el servicio para {machineTitle}.",
                Type = "BOOKING_IN_PROGRESS"
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

        // Envío masivo
        public async Task SendToAllAsync(string title, string message, string type)
        {
            var users = await _userRepo.GetAllAsync();
            var notifications = users.Select(u => new Notification
            {
                UserId = u.Id,
                Title = title,
                Message = message,
                Type = type
            });

            foreach (var n in notifications)
            {
                await _repo.CreateAsync(n);
            }
        }

        // Envío por rol
        public async Task SendToRoleAsync(string role, string title, string message, string type)
        {
            var users = await _userRepo.GetByRoleAsync(role);
            var notifications = users.Select(u => new Notification
            {
                UserId = u.Id,
                Title = title,
                Message = message,
                Type = type
            });

            foreach (var n in notifications)
            {
                await _repo.CreateAsync(n);
            }
        }
    }
}

