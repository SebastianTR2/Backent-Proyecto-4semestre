using Machly.Api.DTOs;
using Machly.Api.Models;
using Machly.Api.Services;
using Machly.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Machly.Api.Controllers
{
    [ApiController]
    [Route("notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsController(NotificationService service)
        {
            _service = service;
        }

        // GET /notifications/{userId} - Obtener notificaciones del usuario
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotifications(string userId)
        {
            var currentUserId = ClaimsHelper.GetUserId(User);
            var role = ClaimsHelper.GetUserRole(User);

            if (currentUserId != userId && role != "ADMIN")
                return Forbid();

            var notifications = await _service.GetByUserAsync(userId);
            return Ok(notifications);
        }

        // POST /notifications/send - Enviar notificación (ADMIN o sistema)
        [HttpPost("send")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type
            };

            await _service.CreateAsync(notification);
            return Ok(notification);
        }

        // PUT /notifications/{id}/read - Marcar como leída
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var result = await _service.MarkAsReadAsync(id);
            if (!result)
                return NotFound();

            return Ok(new { message = "Notification marked as read" });
        }
    }
}

