using Machly.Api.Models;
using Machly.Api.Repositories;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatRepository _repo;

        public ChatController(ChatRepository repo)
        {
            _repo = repo;
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetHistory(string otherUserId)
        {
            var currentUserId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized();

            var messages = await _repo.GetConversationAsync(currentUserId, otherUserId);
            return Ok(messages);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetByBooking(string bookingId)
        {
            var messages = await _repo.GetByBookingAsync(bookingId);
            return Ok(messages);
        }
    }
}
