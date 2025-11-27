using Machly.Api.Models;
using Machly.Api.DTOs;
using Machly.Api.GraphQL.DTOs;
using Machly.Api.Services;
using Machly.Api.GraphQL.Inputs;
using HotChocolate;
using HotChocolate.Authorization;
using System.Security.Claims;

namespace Machly.Api.GraphQL.Mutations
{
    public class Mutation
    {
        [Authorize(Roles = new[] { "RENTER" })]
        public async Task<BookingDto> CreateBooking(
            CreateBookingInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] BookingService bookingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("User not authenticated");
            }

            try 
            {
                var request = new BookingCreateRequest
                {
                    RenterId = userId,
                    MachineId = input.MachineId,
                    Start = input.Start,
                    End = input.End
                };
                var booking = await bookingService.CreateAsync(request);
                if (booking == null) throw new GraphQLException("Booking creation failed");

                return MapBooking(booking);
            }
            catch (Exception ex)
            {
                throw new GraphQLException(ex.Message);
            }
        }

        [Authorize(Roles = new[] { "PROVIDER" })]
        public async Task<MachineDto> CreateMachine(
            CreateMachineInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] MachineService machineService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new GraphQLException("User not authenticated");
            }

            var machine = new Machine
            {
                ProviderId = userId,
                Title = input.Title,
                Description = input.Description,
                PricePerDay = input.PricePerDay,
                Category = input.Category,
                Type = input.Type,
                Lat = input.Lat,
                Lng = input.Lng,
                // Location and GeoLocation will be handled by MachineService
                Photos = input.Photos.Select(url => new MachinePhoto { Url = url }).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await machineService.CreateAsync(machine);
            return MapMachine(machine);
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
                Lat = m.Lat,
                Lng = m.Lng,
                Photos = m.Photos.Select(p => p.Url).ToList(),
                IsOutOfService = m.IsOutOfService,
                RatingAvg = m.RatingAvg,
                RatingCount = m.RatingCount
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
    }
}
