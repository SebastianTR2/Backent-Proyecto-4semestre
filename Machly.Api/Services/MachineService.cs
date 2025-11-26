using Machly.Api.Models;
using Machly.Api.Repositories;

namespace Machly.Api.Services
{
    public class MachineService
    {
        private readonly MachineRepository _repo;

        public MachineService(MachineRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Machine>> GetAllAsync() => _repo.GetAllAsync();
        public Task<Machine?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
        public Task<List<Machine>> GetByProviderAsync(string providerId) => _repo.GetByProviderAsync(providerId);
        
        public Task<List<Machine>> GetFilteredAsync(
            double? lat = null,
            double? lng = null,
            double? radiusKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? type = null,
            string? category = null,
            bool? withOperator = null,
            string? providerId = null) =>
            _repo.GetFilteredAsync(lat, lng, radiusKm, minPrice, maxPrice, type, category, withOperator, providerId);

        public Task CreateAsync(Machine machine) => _repo.CreateAsync(machine);
        public Task<bool> UpdateAsync(string id, Machine machine) => _repo.UpdateAsync(id, machine);
        public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
    }
}
