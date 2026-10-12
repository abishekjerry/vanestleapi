using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using VAapiNestle.Configuration;

namespace VAapiNestle.Services
{
    public interface IEmailService
    {
        Task<bool> SendNotificationEmail(HttpContext context, string subject, string body, string emailsTo);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<bool> SendNotificationEmail(HttpContext context, string subject, string body, string emailsTo) // For Ebiz and PRD
        {
            try
            {
                var host = context.Request.Host.Host;
                if (_settings.RedirectHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
                {
                    emailsTo = _settings.RedirectTo;
                }

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.FromAddress, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                foreach (var receiver in emailsTo.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    mailMessage.To.Add(new MailAddress(receiver));
                }

                using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password)
                };
                await smtpClient.SendMailAsync(mailMessage, context.RequestAborted);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification email '{Subject}'", subject);
                return false;
            }
        }
    }
}
