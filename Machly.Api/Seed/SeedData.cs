using Machly.Api.Models;
using Machly.Api.Repositories;

namespace Machly.Api.Seed
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            UserRepository userRepo,
            MachineRepository machineRepo,
            BookingRepository bookingRepo)
        {
            // Si ya hay usuarios, no hacer nada
            var existingUsers = await userRepo.GetAllAsync();
            if (existingUsers.Any())
                return;

            // ─────────────────────────────
            // 1️⃣ CREAR USUARIOS
            // ─────────────────────────────

            var admin = new User
            {
                Name = "Administrador",
                Email = "admin@machly.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "ADMIN",
                CreatedAt = DateTime.UtcNow
            };

            var provider1 = new User
            {
                Name = "Sebastian",
                Email = "seb@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Provider123"),
                Role = "PROVIDER",
                IsVerifiedProvider = true,
                CreatedAt = DateTime.UtcNow
            };

            var provider2 = new User
            {
                Name = "Proveedor Demo",
                Email = "prov@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Provider123"),
                Role = "PROVIDER",
                IsVerifiedProvider = true,
                CreatedAt = DateTime.UtcNow
            };

            var renter1 = new User
            {
                Name = "Juan",
                Email = "juan@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Renter123"),
                Role = "RENTER",
                CreatedAt = DateTime.UtcNow
            };

            var renter2 = new User
            {
                Name = "Mario",
                Email = "mario@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Renter123"),
                Role = "RENTER",
                CreatedAt = DateTime.UtcNow
            };

            await userRepo.CreateAsync(admin);
            await userRepo.CreateAsync(provider1);
            await userRepo.CreateAsync(provider2);
            await userRepo.CreateAsync(renter1);
            await userRepo.CreateAsync(renter2);

            // IDs actualizados desde Mongo
            var prov1Id = provider1.Id!;
            var prov2Id = provider2.Id!;
            var renter1Id = renter1.Id!;
            var renter2Id = renter2.Id!;


            // ─────────────────────────────
            // 2️⃣ CREAR MÁQUINAS
            // ─────────────────────────────

            var machines = new List<Machine>
            {
                new Machine
                {
                    ProviderId = prov1Id,
                    Title = "Retroexcavadora CAT 420F",
                    Description = "Equipo ideal para construcción pesada.",
                    Type = "URBANA",
                    Category = "Retroexcavadora",
                    PricePerDay = 1200,
                    WithOperator = true,
                    Lat = -17.78,
                    Lng = -63.18,
                    Photos = new List<MachinePhoto>
                    {
                        new MachinePhoto { Url = "https://cdn.machly/retro1.jpg", IsCover = true }
                    },
                    TariffsAgro = null
                },

                new Machine
                {
                    ProviderId = prov2Id,
                    Title = "Tractor John Deere 5075E",
                    Description = "Tractor agrícola de gran potencia.",
                    Type = "AGRICOLA",
                    Category = "Tractor",
                    PricePerDay = 900,
                    WithOperator = false,
                    Lat = -17.80,
                    Lng = -63.20,
                    Photos = new(),
                    TariffsAgro = new TariffAgro
                    {
                        Hectarea = 150,
                        Tonelada = 45,
                        KmTariffs = new List<KmTariff>
                        {
                            new KmTariff { MinKm = 0, MaxKm = 50, TarifaPorKm = 5 }
                        }
                    }
                }
            };

            foreach (var m in machines)
                await machineRepo.CreateAsync(m);

            var machine1 = machines[0];
            var machine2 = machines[1];


            // ─────────────────────────────
            // 3️⃣ CREAR RESERVAS
            // ─────────────────────────────

            var bookings = new List<Booking>
            {
                new Booking
                {
                    MachineId = machine1.Id!,
                    RenterId = renter1Id,
                    ProviderId = machine1.ProviderId,
                    Start = DateTime.UtcNow.AddDays(1),
                    End = DateTime.UtcNow.AddDays(3),
                    Status = "CONFIRMED",
                    TotalPrice = machine1.PricePerDay * 2
                },
                new Booking
                {
                    MachineId = machine2.Id!,
                    RenterId = renter2Id,
                    ProviderId = machine2.ProviderId,
                    Start = DateTime.UtcNow.AddDays(5),
                    End = DateTime.UtcNow.AddDays(7),
                    Status = "PENDING",
                    TotalPrice = machine2.PricePerDay * 2
                }
            };

            foreach (var b in bookings)
                await bookingRepo.CreateAsync(b);
        }
    }
}
