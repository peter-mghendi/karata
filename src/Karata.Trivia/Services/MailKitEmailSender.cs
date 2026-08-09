using Karata.Trivia.Models.Configuration;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Karata.Trivia.Services;

public class MailKitEmailSender(ILogger<MailKitEmailSender> logger, IOptions<MailSettings> options)
    : IEmailSender
{
    private readonly MailSettings _settings = options.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            using var emailMessage = new MimeMessage();
            var emailFrom = new MailboxAddress(_settings.SenderName, _settings.SenderEmail);
            emailMessage.From.Add(emailFrom);
            var emailTo = new MailboxAddress(email, email);
            emailMessage.To.Add(emailTo);

            emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "cc@example.com"));
            emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

            emailMessage.Subject = subject;

            var emailBodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };

            emailMessage.Body = emailBodyBuilder.ToMessageBody();
                
            using var mailClient = new SmtpClient();
            await mailClient.ConnectAsync(_settings.Server, _settings.Port, MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable);
            await mailClient.AuthenticateAsync(_settings.UserName, _settings.Password);
            await mailClient.SendAsync(emailMessage);
            await mailClient.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Sender}: {Subject}.", email, subject);
        }
    }
}