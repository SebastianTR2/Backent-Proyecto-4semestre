namespace Machly.Api.Services
{
    public class MockNotificationSender : INotificationSender
    {
        private readonly ILogger<MockNotificationSender> _logger;

        public MockNotificationSender(ILogger<MockNotificationSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            _logger.LogInformation($"[MOCK EMAIL] To: {to} | Subject: {subject} | Body: {body}");
            return Task.CompletedTask;
        }

        public Task SendWhatsAppAsync(string to, string message)
        {
            _logger.LogInformation($"[MOCK WHATSAPP] To: {to} | Message: {message}");
            return Task.CompletedTask;
        }
    }
}
