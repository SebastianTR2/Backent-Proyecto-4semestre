using Machly.Api.DTOs;
using Machly.Api.Models;
using Machly.Api.Repositories;
using Machly.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("admin")]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly UserRepository _userRepo;
        private readonly MachineService _machineService;
        private readonly BookingService _bookingService;
        private readonly MongoDbContext _context;

        public AdminController(
            UserRepository userRepo,
            MachineService machineService,
            BookingService bookingService,
            MongoDbContext context)
        {
            _userRepo = userRepo;
            _machineService = machineService;
            _bookingService = bookingService;
            _context = context;
        }

        // GET /admin/users - Listar todos los usuarios
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.GetCollection<User>("users")
                .Find(_ => true)
                .SortByDescending(u => u.CreatedAt)
                .ToListAsync();
            return Ok(users);
        }

        // GET /admin/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var dto = new AdminUserDetailsDto
            {
                User = MapUser(user)
            };

            if (user.Role == "PROVIDER")
            {
                var machines = await _machineService.GetByProviderAsync(user.Id!);
                dto.ProviderMachines = machines.Select(m => new AdminMachineSummaryDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Type = m.Type,
                    Category = m.Category,
                    PricePerDay = m.PricePerDay,
                    WithOperator = m.WithOperator,
                    RatingAvg = m.RatingAvg,
                    RatingCount = m.RatingCount,
                    CreatedAt = m.CreatedAt
                }).ToList();
            }

            if (user.Role == "RENTER")
            {
                var bookings = await _bookingService.GetByUserAsync(user.Id!);
                var machineTitles = new Dictionary<string, string>();
                foreach (var booking in bookings)
                {
                    if (!machineTitles.ContainsKey(booking.MachineId))
                    {
                        var machine = await _machineService.GetByIdAsync(booking.MachineId);
                        machineTitles[booking.MachineId] = machine?.Title ?? "Máquina";
                    }
                }

                dto.RenterBookings = bookings.Select(b => new AdminBookingSummaryDto
                {
                    Id = b.Id,
                    MachineId = b.MachineId,
                    MachineTitle = machineTitles.GetValueOrDefault(b.MachineId, "Máquina"),
                    Start = b.Start,
                    End = b.End,
                    Status = b.Status,
                    TotalPrice = b.TotalPrice
                }).ToList();
            }

            return Ok(dto);
        }

        // GET /admin/machines - Listar máquinas con filtros
        [HttpGet("machines")]
        public async Task<IActionResult> GetMachines(
            [FromQuery] string? type,
            [FromQuery] string? category,
            [FromQuery] string? providerId,
            [FromQuery] bool? withOperator,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var machines = await _machineService.GetFilteredAsync(
                type: type,
                category: category,
                providerId: providerId,
                withOperator: withOperator,
                minPrice: minPrice,
                maxPrice: maxPrice);

            var providerIds = machines.Select(m => m.ProviderId).Distinct().ToList();
            var providers = await _context.GetCollection<User>("users")
                .Find(u => providerIds.Contains(u.Id!))
                .ToListAsync();

            var result = machines.Select(m =>
            {
                var provider = providers.FirstOrDefault(p => p.Id == m.ProviderId);
                return new
                {
                    m.Id,
                    m.Title,
                    m.Description,
                    m.PricePerDay,
                    m.Type,
                    m.Category,
                    m.WithOperator,
                    m.ProviderId,
                    ProviderName = provider?.Name ?? "Unknown",
                    ProviderEmail = provider?.Email ?? "Unknown"
                };
            });

            return Ok(result);
        }

        // GET /admin/machines/{id}
        [HttpGet("machines/{id}")]
        public async Task<IActionResult> GetMachineDetails(string id)
        {
            var machine = await _machineService.GetByIdAsync(id);
            if (machine == null)
                return NotFound();

            var provider = await _userRepo.GetByIdAsync(machine.ProviderId);
            var bookings = await _bookingService.GetByMachineAsync(machine.Id!);
            var renterIds = bookings.Where(b => b.Review != null)
                .Select(b => b.RenterId)
                .Distinct()
                .ToList();
            var renters = renterIds.Any()
                ? await _context.GetCollection<User>("users")
                    .Find(u => renterIds.Contains(u.Id!))
                    .ToListAsync()
                : new List<User>();

            var dto = new AdminMachineDetailsDto
            {
                Machine = machine,
                Provider = provider != null ? MapUser(provider) : null,
                Reviews = bookings
                    .Where(b => b.Review != null)
                    .Select(b => new AdminBookingReviewDto
                    {
                        BookingId = b.Id,
                        RenterId = b.RenterId,
                        RenterName = renters.FirstOrDefault(r => r.Id == b.RenterId)?.Name,
                        Review = b.Review,
                        CreatedAt = b.CreatedAt
                    }).ToList()
            };

            return Ok(dto);
        }

        // DELETE /admin/machines/{id}
        [HttpDelete("machines/{id}")]
        public async Task<IActionResult> DeleteMachine(string id)
        {
            var machine = await _machineService.GetByIdAsync(id);
            if (machine == null)
                return NotFound();

            var deleted = await _machineService.DeleteAsync(id);
            if (!deleted)
                return BadRequest("Failed to delete machine");

            return Ok(new { message = "Machine deleted", id });
        }

        // GET /admin/bookings - Listar reservas con filtros de fecha
        [HttpGet("bookings")]
        public async Task<IActionResult> GetBookings([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var bookings = await _bookingService.GetAllAsync(from, to);
            return Ok(bookings);
        }

        // PUT /admin/provider/verify/{id} - Validar proveedor
        [HttpPut("provider/verify/{id}")]
        public async Task<IActionResult> VerifyProvider(string id, [FromBody] ProviderVerificationRequest request)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.Role != "PROVIDER")
                return BadRequest("User is not a provider");

            user.IsVerifiedProvider = request?.IsVerified ?? true;
            await _userRepo.UpdateAsync(user);

            return Ok(new
            {
                user.Id,
                user.IsVerifiedProvider
            });
        }

        // GET /admin/reports/basic - Reportes básicos
        [HttpGet("reports/basic")]
        public async Task<IActionResult> GetBasicReports()
        {
            var users = await _context.GetCollection<User>("users").Find(_ => true).ToListAsync();
            var machines = await _machineService.GetAllAsync();
            var bookings = await _bookingService.GetAllAsync();

            var report = new
            {
                TotalUsers = users.Count,
                TotalProviders = users.Count(u => u.Role == "PROVIDER"),
                TotalRenters = users.Count(u => u.Role == "RENTER"),
                TotalMachines = machines.Count,
                TotalBookings = bookings.Count,
                ActiveBookings = bookings.Count(b => b.Status == "CONFIRMED" || b.Status == "IN_PROGRESS"),
                TotalRevenue = bookings.Sum(b => b.TotalPrice)
            };

            return Ok(report);
        }

        private static AdminUserSummaryDto MapUser(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsVerifiedProvider = user.IsVerifiedProvider,
            CreatedAt = user.CreatedAt
        };
    }
}

