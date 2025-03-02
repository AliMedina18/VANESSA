using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _configuration;

    // Constructor donde se inyecta la configuración
    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    // Método para enviar el correo
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Obtener la configuración del correo desde appsettings.json
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpServer = emailSettings["SmtpServer"] ?? throw new InvalidOperationException("SmtpServer no está configurado.");

        // Validar el puerto SMTP antes de convertirlo
        if (!int.TryParse(emailSettings["SmtpPort"], out int smtpPort))
        {
            throw new InvalidOperationException("SmtpPort no es un número válido.");
        }

        var senderEmail = emailSettings["SenderEmail"] ?? throw new InvalidOperationException("SenderEmail no está configurado.");
        var senderPassword = emailSettings["SenderPassword"] ?? throw new InvalidOperationException("SenderPassword no está configurado.");
        var senderName = emailSettings["SenderName"] ?? "Remitente";

        // Crear el mensaje que se enviará
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail)); // Agregar el remitente
        message.To.Add(new MailboxAddress("Nombre Destinatario", toEmail)); // Agregar el destinatario
        message.Subject = subject; // Asunto del correo

        // Cuerpo del correo
        var bodyBuilder = new BodyBuilder { HtmlBody = body }; // Aquí puedes incluir HTML si lo necesitas
        message.Body = bodyBuilder.ToMessageBody(); // Asignar el cuerpo del correo

        // Usar SmtpClient para enviar el correo
        using (var client = new SmtpClient())
        {
            // Conectar al servidor SMTP
            await client.ConnectAsync(smtpServer, smtpPort, false);
            // Autenticar con las credenciales
            await client.AuthenticateAsync(senderEmail, senderPassword);
            // Enviar el correo
            await client.SendAsync(message);
            // Desconectar
            await client.DisconnectAsync(true);
        }
    }
}
