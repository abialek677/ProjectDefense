using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ProjectDefense.Web.Services.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly ILogger<SendGridEmailSender> _logger;
        private readonly SendGridOptions _options;
        
        public SendGridEmailSender(
            IOptions<SendGridOptions> options,
            ILogger<SendGridEmailSender> logger)
        {
            _options = options.Value;
            _logger = logger;
        }
        
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            Console.WriteLine($"Sending email: {email}");
            if (string.IsNullOrEmpty(_options.ApiKey))
            {
                throw new Exception("SendGrid API key is not configured");
            }
            
            var client = new SendGridClient(_options.ApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            
            msg.AddTo(new EmailAddress(email));
            msg.SetClickTracking(false, false);
            
            var response = await client.SendEmailAsync(msg);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to send email. Status: {response.StatusCode}");
            }
        }
    }
    
    public class SendGridOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }
}