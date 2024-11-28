using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Vanessa.Data;
using Vanessa.Models;

public class RecuperacionService
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public RecuperacionService(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> SolicitarRecuperacionAsync(string correo, IUrlHelper urlHelper)
    {
        // Buscar el usuario por correo
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo == correo && u.Activo);

        if (usuario == null)
            return false; // Usuario no encontrado

        // Generar un token único
        var token = Guid.NewGuid().ToString();
        usuario.TokenRecuperacion = token;
        usuario.TokenExpiracion = DateTime.Now.AddHours(1); // Token válido por 1 hora

        // Guardar el token en la base de datos
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();

        // Construir la URL absoluta
        var enlace = urlHelper.Action(
            action: "Restablecer",
            controller: "RecuperacionContraseña",
            values: new { token },
            protocol: "https" // Usa "http" o "https" según sea necesario
        );

        // Crear el contenido del correo
        var cuerpoCorreo = $"Haz clic en el siguiente enlace para restablecer tu contraseña: <a href='{enlace}'>Restablecer Contraseña</a>";

        // Enviar el correo
        await _emailService.SendEmailAsync(correo, "Recuperación de Contraseña", cuerpoCorreo);

        return true;
    }

    public bool RestablecerContraseña(string token, string nuevaContraseña)
    {
        // Validar el token
        var usuario = _context.Usuarios
            .FirstOrDefault(u => u.TokenRecuperacion == token && u.TokenExpiracion > DateTime.Now);

        if (usuario == null)
        {
            Console.WriteLine("Token inválido o expirado.");
            return false; // Token inválido o expirado
        }

        // Generar hash de la nueva contraseña
        var hashSalt = ConvertirContraseña(nuevaContraseña);
        usuario.Contraseña = hashSalt;

        // Invalidar el token después de su uso
        usuario.TokenRecuperacion = null;
        usuario.TokenExpiracion = null;

        try
        {
            // Actualizar el usuario en la base de datos
            _context.Usuarios.Update(usuario);
            _context.SaveChanges();
            Console.WriteLine("Contraseña actualizada correctamente.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar los cambios: {ex.Message}");
            return false;
        }
    }


    private string ConvertirContraseña(string contraseña)
    {
        // Método para convertir la contraseña en hash con salt
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);

        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: contraseña,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));

        return $"{Convert.ToBase64String(salt)}.{hash}";
    }
}
