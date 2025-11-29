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

        // GET /admin/users - Listar todos los usuarios con filtro de rol
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? role)
        {
            var filter = Builders<User>.Filter.Empty;
            if (!string.IsNullOrEmpty(role))
            {
                filter = Builders<User>.Filter.Eq(u => u.Role, role);
            }

            var users = await _context.GetCollection<User>("users")
                .Find(filter)
                .SortByDescending(u => u.CreatedAt)
                .ToListAsync();
            
            var dtos = users.Select(MapUser).ToList();
            return Ok(dtos);
        }

        // POST /admin/users - Crear usuario
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            var existing = await _userRepo.GetByEmailAsync(user.Email);
            if (existing != null)
                return Conflict("Email already exists");

            // Simple validation
            if (!user.Email.Contains("@") || !user.Email.Contains(".com")) // Basic check as requested
                return BadRequest("Invalid email format");

            // Hash password if provided, else default? 
            // The UI sends a password.
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }
            else
            {
                // Default password if none provided? Or require it?
                // For admin creation, usually we require it.
                return BadRequest("Password is required");
            }

            user.CreatedAt = DateTime.UtcNow;
            await _userRepo.CreateAsync(user);
            return CreatedAtAction(nameof(GetUserDetails), new { id = user.Id }, MapUser(user));
        }

        // PUT /admin/users/{id} - Actualizar usuario
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
        {
            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.IsVerifiedProvider = user.IsVerifiedProvider;

            // Update password only if provided
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            await _userRepo.UpdateAsync(existing);
            return Ok(MapUser(existing));
        }

        // DELETE /admin/users/{id} - Eliminar usuario
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userRepo.DeleteAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "User deleted" });
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
                dto.ProviderMachines = machines.Select(m => new AdminMachineListItem
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Type = m.Type,
                    Category = m.Category,
                    PricePerDay = m.PricePerDay,
                    WithOperator = m.WithOperator,
                    ProviderId = user.Id!,
                    ProviderName = user.Name,
                    ProviderEmail = user.Email,
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

                dto.RenterBookings = bookings.Select(b => new AdminBookingListItem
                {
                    Id = b.Id,
                    MachineId = b.MachineId,
                    MachineTitle = machineTitles.GetValueOrDefault(b.MachineId, "Máquina"),
                    RenterId = user.Id!,
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
                return new AdminMachineListItem
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    PricePerDay = m.PricePerDay,
                    Type = m.Type,
                    Category = m.Category,
                    WithOperator = m.WithOperator,
                    ProviderId = m.ProviderId,
                    ProviderName = provider?.Name ?? "Unknown",
                    ProviderEmail = provider?.Email ?? "Unknown",
                    RatingAvg = m.RatingAvg,
                    RatingCount = m.RatingCount,
                    CreatedAt = m.CreatedAt
                };
            }).ToList();

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
            
            // Necesitamos títulos de máquinas para el DTO
            var machineIds = bookings.Select(b => b.MachineId).Distinct().ToList();
            var machines = await _context.GetCollection<Machine>("machines")
                .Find(m => machineIds.Contains(m.Id!))
                .Project(m => new { m.Id, m.Title })
                .ToListAsync();
            var machineTitles = machines.ToDictionary(m => m.Id!, m => m.Title);

            var dtos = bookings.Select(b => new AdminBookingListItem
            {
                Id = b.Id,
                MachineId = b.MachineId,
                MachineTitle = machineTitles.GetValueOrDefault(b.MachineId, "Máquina"),
                RenterId = b.RenterId,
                Start = b.Start,
                End = b.End,
                Status = b.Status,
                TotalPrice = b.TotalPrice
            }).ToList();

            return Ok(dtos);
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

        // GET /admin/dashboard - Dashboard completo con estadísticas reales
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var users = await _context.GetCollection<User>("users").Find(_ => true).ToListAsync();
            var machines = await _machineService.GetAllAsync();
            var bookings = await _bookingService.GetAllAsync();

            // Estadísticas básicas
            var totalUsers = users.Count;
            var totalProviders = users.Count(u => u.Role == "PROVIDER");
            var totalRenters = users.Count(u => u.Role == "RENTER");
            var totalMachines = machines.Count;
            var totalBookings = bookings.Count;

            // Reservas últimos 30 días
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var bookingsLast30Days = bookings.Count(b => b.CreatedAt >= thirtyDaysAgo);

            // Top Categorías (basado en máquinas reservadas)
            // 1. Obtener IDs de máquinas reservadas
            var bookedMachineIds = bookings.Select(b => b.MachineId).ToList();
            
            // 2. Unir con máquinas para obtener categorías
            var categoryStats = bookings
                .Join(machines, b => b.MachineId, m => m.Id, (b, m) => m.Category)
                .GroupBy(c => c)
                .Select(g => new CategoryStat { Category = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // Top Máquinas (más reservadas)
            var machineStats = bookings
                .GroupBy(b => b.MachineId)
                .Select(g => new 
                { 
                    MachineId = g.Key, 
                    Count = g.Count() 
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Join(machines, s => s.MachineId, m => m.Id, (s, m) => new MachineStat 
                { 
                    MachineTitle = m.Title, 
                    BookingsCount = s.Count 
                })
                .ToList();

            var dto = new AdminDashboardDto
            {
                TotalUsers = totalUsers,
                TotalProviders = totalProviders,
                TotalRenters = totalRenters,
                TotalMachines = totalMachines,
                TotalBookings = totalBookings,
                BookingsLast30Days = bookingsLast30Days,
                TopCategories = categoryStats,
                TopMachines = machineStats
            };

            return Ok(dto);
        }

        private static ApiAdminUserSummaryDto MapUser(User user) => new()
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            IsVerifiedProvider = user.IsVerifiedProvider,
            CreatedAt = user.CreatedAt
        };
        // EXPORT
        [HttpGet("export/users/csv")]
        public async Task<IActionResult> ExportUsersCsv()
        {
            var users = await _userRepo.GetAllAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Id,Name,Email,Role,IsVerified");
            foreach (var u in users)
            {
                csv.AppendLine($"{u.Id},{u.Name},{u.Email},{u.Role},{u.IsVerifiedProvider}");
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "users.csv");
        }

        [HttpGet("export/machines/csv")]
        public async Task<IActionResult> ExportMachinesCsv()
        {
            var machines = await _machineService.GetAllAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Id,Title,Type,Category,Price,ProviderId");
            foreach (var m in machines)
            {
                csv.AppendLine($"{m.Id},{m.Title},{m.Type},{m.Category},{m.PricePerDay},{m.ProviderId}");
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "machines.csv");
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromServices] AuditService auditService, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? userId, [FromQuery] string? action)
        {
            var logs = await auditService.GetLogsAsync(from, to, userId, action);
            return Ok(logs);
        }
        // GET /admin/reports/income
        [HttpGet("reports/income")]
        public async Task<IActionResult> GetIncomeReport([FromQuery] int year)
        {
            var bookings = await _bookingService.GetAllAsync();
            
            // Filter by year and completed/confirmed status
            var relevantBookings = bookings
                .Where(b => b.Start.Year == year && (b.Status == "COMPLETED" || b.Status == "CONFIRMED"))
                .ToList();

            // Group by month
            var monthlyIncome = relevantBookings
                .GroupBy(b => b.Start.Month)
                .Select(g => new 
                { 
                    Month = g.Key, 
                    Total = g.Sum(b => b.TotalPrice) 
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Ensure all months are represented
            var result = Enumerable.Range(1, 12).Select(month => new
            {
                Month = month,
                Total = monthlyIncome.FirstOrDefault(x => x.Month == month)?.Total ?? 0
            }).ToList();

            return Ok(result);
        }

        // GET /admin/reports/occupancy
        [HttpGet("reports/occupancy")]
        public async Task<IActionResult> GetOccupancyReport()
        {
            var machines = await _machineService.GetAllAsync();
            var bookings = await _bookingService.GetAllAsync();

            var totalMachines = machines.Count;
            if (totalMachines == 0) return Ok(new List<object>());

            // Top 10 machines by booking count
            var occupancy = bookings
                .GroupBy(b => b.MachineId)
                .Select(g => new
                {
                    MachineId = g.Key,
                    BookingCount = g.Count()
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(10)
                .Join(machines, 
                    occ => occ.MachineId, 
                    mac => mac.Id, 
                    (occ, mac) => new 
                    { 
                        MachineTitle = mac.Title, 
                        Count = occ.BookingCount 
                    })
                .ToList();

            return Ok(occupancy);
        }
    }
}

