using Machly.Api.Models;
using Machly.Api.Repositories;
using Microsoft.AspNetCore.SignalR;

namespace Machly.Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatRepository _repo;

        public ChatHub(ChatRepository repo)
        {
            _repo = repo;
        }

        public async Task SendMessage(string senderId, string receiverId, string message, string? bookingId)
        {
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                BookingId = bookingId,
                Timestamp = DateTime.UtcNow
            };

            await _repo.CreateAsync(chatMessage);

            // Enviar al receptor (usando su ID como grupo o UserIdentifier si está autenticado)
            // Asumimos que los clientes se unen a un grupo con su UserId
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message, bookingId, chatMessage.Timestamp);
            
            // También enviar de vuelta al remitente para confirmación/UI
            await Clients.Caller.SendAsync("MessageSent", chatMessage);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
