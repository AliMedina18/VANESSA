using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Vanessa.Interfaces;

namespace Vanessa.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _configuration.GetSection("EmailSettings");
            var smtpServer   = settings["SmtpServer"]   ?? "smtp.gmail.com";
            var senderEmail  = settings["SenderEmail"]  ?? string.Empty;
            var senderPass   = settings["SenderPassword"] ?? string.Empty;
            var senderName   = settings["SenderName"]   ?? "EpicSoft18";

            if (!int.TryParse(settings["SmtpPort"], out int smtpPort))
                smtpPort = 587;

            // Sin credenciales configuradas: solo registrar advertencia y no fallar
            if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPass))
            {
                _logger.LogWarning("EmailService: credenciales SMTP no configuradas. Correo a {Email} no enviado.", toEmail);
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(string.Empty, toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

                using var client = new SmtpClient();
                // Puerto 587 requiere StartTls (no SSL directo)
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, senderPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // El error de correo no rompe el flujo de la aplicación
                _logger.LogError(ex, "Error al enviar correo a {Email}", toEmail);
            }
        }
    }
}
