using Machly.Api.Models;
using Machly.Api.Repositories;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("favorites")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoriteRepository _repo;

        public FavoritesController(FavoriteRepository repo)
        {
            _repo = repo;
        }

        // POST /favorites/{machineId}
        [HttpPost("{machineId}")]
        public async Task<IActionResult> AddFavorite(string machineId)
        {
            var userId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var favorite = new Favorite
            {
                UserId = userId,
                MachineId = machineId
            };

            await _repo.CreateAsync(favorite);
            return Ok(new { message = "Added to favorites" });
        }

        // DELETE /favorites/{machineId}
        [HttpDelete("{machineId}")]
        public async Task<IActionResult> RemoveFavorite(string machineId)
        {
            var userId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _repo.DeleteAsync(userId, machineId);
            return Ok(new { message = "Removed from favorites" });
        }

        // GET /favorites/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserFavorites(string userId)
        {
            var currentUserId = ClaimsHelper.GetUserId(User);
            if (currentUserId != userId) return Forbid();

            var favorites = await _repo.GetByUserAsync(userId);
            return Ok(favorites);
        }
    }
}
