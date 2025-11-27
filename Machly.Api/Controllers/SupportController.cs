using Machly.Api.Models;
using Machly.Api.Repositories;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("support")]
    [Authorize]
    public class SupportController : ControllerBase
    {
        private readonly SupportTicketRepository _repo;

        public SupportController(SupportTicketRepository repo)
        {
            _repo = repo;
        }

        // POST /support
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupportTicket ticket)
        {
            var userId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            ticket.UserId = userId;
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.Status = "OPEN";
            
            await _repo.CreateAsync(ticket);
            return Ok(ticket);
        }

        // GET /support/my-tickets
        [HttpGet("my-tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = ClaimsHelper.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tickets = await _repo.GetByUserAsync(userId);
            return Ok(tickets);
        }

        // GET /support/all - ADMIN ONLY
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var tickets = await _repo.GetAllAsync();
            return Ok(tickets);
        }

        // GET /support/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            var userId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (role != "ADMIN" && ticket.UserId != userId)
                return Forbid();

            return Ok(ticket);
        }

        // PUT /support/{id}/resolve - ADMIN ONLY
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Resolve(string id, [FromBody] ResolveTicketRequest request)
        {
            var ticket = await _repo.GetByIdAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = "RESOLVED";
            ticket.AdminResponse = request.Response;
            ticket.ResolvedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(id, ticket);
            return Ok(ticket);
        }
    }

    public class ResolveTicketRequest
    {
        public string Response { get; set; } = null!;
    }
}
