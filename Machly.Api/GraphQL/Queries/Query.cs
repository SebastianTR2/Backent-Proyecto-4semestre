using Machly.Api.Models;
using Machly.Api.DTOs;
using Machly.Api.GraphQL.DTOs;
using Machly.Api.Repositories;
using HotChocolate;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace Machly.Api.GraphQL.Queries
{
    public class Query
    {
        // --- MACHINES ---

        [Authorize]
        public async Task<List<MachineDto>> GetMachines(
            [Service] MachineRepository machineRepo)
        {
            var machines = await machineRepo.GetAllAsync();
            return machines.Select(MapMachine).ToList();
        }

        [Authorize]
        public async Task<MachineDto?> GetMachineById(
            string id,
            [Service] MachineRepository machineRepo)
        {
            var machine = await machineRepo.GetByIdAsync(id);
            return machine == null ? null : MapMachine(machine);
        }

        [Authorize(Roles = new[] { "PROVIDER" })]
        public async Task<List<MachineDto>> GetMyMachines(
            ClaimsPrincipal claimsPrincipal,
            [Service] MachineRepository machineRepo)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return new List<MachineDto>();

            var machines = await machineRepo.GetByProviderAsync(userId);
            return machines.Select(MapMachine).ToList();
        }

        // --- BOOKINGS ---

        [Authorize]
        public async Task<List<BookingDto>> GetMyBookings(
            ClaimsPrincipal claimsPrincipal,
            [Service] BookingRepository bookingRepo)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return new List<BookingDto>();

            if (claimsPrincipal.IsInRole("PROVIDER"))
            {
                return new List<BookingDto>(); 
            }
            
            var bookings = await bookingRepo.GetByUserAsync(userId);
            return bookings.Select(MapBooking).ToList();
        }

        [Authorize(Roles = new[] { "PROVIDER" })]
        public async Task<List<BookingDto>> GetProviderBookings(
            ClaimsPrincipal claimsPrincipal,
            [Service] BookingRepository bookingRepo)
        {
             // Placeholder for now as we need complex injection for the real repo method
             return new List<BookingDto>();
        }

        // --- USERS ---

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<List<UserDto>> GetUsers(
            [Service] UserRepository userRepo)
        {
            var users = await userRepo.GetAllAsync();
            return users.Select(MapUser).ToList();
        }

        [Authorize]
        public async Task<UserDto?> Me(
            ClaimsPrincipal claimsPrincipal,
            [Service] UserRepository userRepo)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;
            var user = await userRepo.GetByIdAsync(userId);
            return user == null ? null : MapUser(user);
        }

        // --- MAPPERS ---

        private static MachineDto MapMachine(Machine m)
        {
            return new MachineDto
            {
                Id = m.Id ?? "",
                ProviderId = m.ProviderId,
                Title = m.Title,
                Description = m.Description,
                PricePerDay = m.PricePerDay,
                Category = m.Category,
                Type = m.Type,
                Lat = m.Location?.Lat ?? 0,
                Lng = m.Location?.Lng ?? 0,
                Photos = m.Photos.Select(p => p.Url).ToList(),
                IsOutOfService = m.IsOutOfService,
                RatingAvg = m.RatingAvg,
                RatingCount = m.RatingCount,
                TariffsAgro = m.TariffsAgro == null ? null : new TariffAgroDto
                {
                    Hectarea = m.TariffsAgro.Hectarea,
                    Tonelada = m.TariffsAgro.Tonelada,
                    KmTariffs = m.TariffsAgro.KmTariffs.Select(k => new KmTariffDto
                    {
                        MinKm = k.MinKm,
                        MaxKm = k.MaxKm,
                        TarifaPorKm = k.TarifaPorKm
                    }).ToList()
                }
            };
        }

        private static BookingDto MapBooking(Booking b)
        {
            return new BookingDto
            {
                Id = b.Id ?? "",
                MachineId = b.MachineId,
                RenterId = b.RenterId,
                Start = b.Start,
                End = b.End,
                TotalPrice = b.TotalPrice,
                Status = b.Status,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                ReviewRating = b.Review?.Rating,
                ReviewComment = b.Review?.Comment
            };
        }

        private static UserDto MapUser(User u)
        {
            return new UserDto
            {
                Id = u.Id ?? "",
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                PhotoUrl = u.PhotoUrl
            };
        }
    }
}
