using AngularApi.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AngularApi.UtilityServices
{
    public class EmailServiceClass : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailServiceClass> _logger;

        public EmailServiceClass(IConfiguration config, ILogger<EmailServiceClass> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void EndEmail(Emailmodel emailmodel)
        {
            var emailmsg = new MimeMessage();
            var from = _config["EmailSettings:From"];
            var to = emailmodel.To;
            var subject = emailmodel.Subject;
            var body = emailmodel.Content;

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                _logger.LogError("Email model contains empty fields.");
                throw new ArgumentException("Email model contains empty fields.");
            }

            emailmsg.From.Add(new MailboxAddress("MealBook", from));
            emailmsg.To.Add(new MailboxAddress(to, to));
            emailmsg.Subject = subject;
            emailmsg.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using (var client = new SmtpClient())
            {
                try
                {
                    // For testing purposes, bypass SSL certificate validation
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    var smtpServer = _config["EmailSettings:SmtpServer"];
                    if (string.IsNullOrWhiteSpace(smtpServer))
                    {
                        throw new ArgumentNullException("EmailSettings:SmtpServer", "SMTP server is not configured.");
                    }

                    if (!int.TryParse(_config["EmailSettings:SmtpPort"], out var smtpPort))
                    {
                        throw new ArgumentNullException("EmailSettings:SmtpPort", "SMTP port is not configured or invalid.");
                    }

                    if (!bool.TryParse(_config["EmailSettings:SmtpUseSsl"], out var smtpUseSsl))
                    {
                        throw new ArgumentNullException("EmailSettings:SmtpUseSsl", "SMTP SSL usage is not configured or invalid.");
                    }

                    var smtpUser = _config["EmailSettings:Username"];
                    var smtpPass = _config["EmailSettings:Password"];
                    if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
                    {
                        throw new ArgumentNullException("EmailSettings:Username or EmailSettings:Password", "SMTP username or password is not configured.");
                    }

                    client.Connect(smtpServer, smtpPort, smtpUseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                    client.Authenticate(smtpUser, smtpPass);

                    client.Send(emailmsg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email.");
                    throw new Exception("Failed to send email.", ex);
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
