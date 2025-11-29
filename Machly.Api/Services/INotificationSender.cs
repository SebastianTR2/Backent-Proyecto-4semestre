namespace Machly.Api.Services
{
    public interface INotificationSender
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWhatsAppAsync(string to, string message);
    }
}
