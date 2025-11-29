using Machly.Api.DTOs;
using Machly.Api.Models;
using Machly.Api.Repositories;
using Machly.Api.Utils;

namespace Machly.Api.Services
{
    public class AuthService
    {
        private readonly UserRepository _repo;
        private readonly JwtHelper _jwt;

        public AuthService(UserRepository repo, JwtHelper jwt)
        {
            _repo = repo;
            _jwt = jwt;
        }

        public async Task<string?> RegisterAsync(RegisterRequest request)
        {
            var existing = await _repo.GetByEmailAsync(request.Email);
            if (existing != null)
                return null;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.CreateAsync(user);
            return _jwt.GenerateToken(user);
        }

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var user = await _repo.GetByEmailAsync(request.Email);
            if (user == null)
                return null;

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                return null;

            return _jwt.GenerateToken(user);
        }
    }
}
