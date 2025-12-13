using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using FitApp.Options;

namespace FitApp.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly EmailFromOptions _emailFromOptions;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(
            IOptions<SmtpOptions> smtpOptions,
            IOptions<EmailFromOptions> emailFromOptions,
            ILogger<SmtpEmailSender> logger)
        {
            _smtpOptions = smtpOptions.Value;
            _emailFromOptions = emailFromOptions.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_emailFromOptions.Name, _emailFromOptions.Address));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                
                // Retry logic - 2 attempts
                for (int attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        await smtp.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, 
                            _smtpOptions.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                        
                        await smtp.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Pass);
                        await smtp.SendAsync(email);
                        await smtp.DisconnectAsync(true);
                        
                        _logger.LogInformation("Email sent successfully to {Email} on attempt {Attempt}", to, attempt);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send email to {Email} on attempt {Attempt}", to, attempt);
                        
                        if (attempt == 2)
                        {
                            _logger.LogError(ex, "Failed to send email to {Email} after {Attempts} attempts", to, attempt);
                            throw;
                        }
                        
                        // Wait 1 second before retry
                        await Task.Delay(1000);
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }
    }
}

