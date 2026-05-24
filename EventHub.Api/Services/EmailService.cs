using System.Net;
using System.Net.Mail;
using EventHub.Api.Settings;

namespace EventHub.Api.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, bool isHtml = true);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(recipientEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {recipientEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {recipientEmail}: {ex.Message}");
            return false;
        }
    }
}

