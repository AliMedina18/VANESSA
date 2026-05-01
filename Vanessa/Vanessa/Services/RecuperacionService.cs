using Microsoft.AspNetCore.Mvc;
using Vanessa.Interfaces;
using Vanessa.Repositories;
using Vanessa.Models;
using Microsoft.EntityFrameworkCore;

namespace Vanessa.Services
{
    public class RecuperacionService
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;
        private readonly ILogger<RecuperacionService> _logger;

        public RecuperacionService(
            IUsuarioRepository usuarioRepo,
            IAuthService authService,
            IEmailService emailService,
            ILogger<RecuperacionService> logger)
        {
            _usuarioRepo  = usuarioRepo;
            _authService  = authService;
            _emailService = emailService;
            _logger       = logger;
        }

        public async Task<bool> SolicitarRecuperacionAsync(string correo, IUrlHelper urlHelper)
        {
            var usuario = await _usuarioRepo.GetByCorreoAsync(correo);
            if (usuario == null) return false;

            // Obtener o crear UsuarioAuditoria
            var auditoria = usuario.Auditoria ?? new UsuarioAuditoria { UsuarioId = usuario.Id };
            auditoria.TokenRecuperacion = Guid.NewGuid().ToString();
            auditoria.TokenExpiracion = DateTime.UtcNow.AddHours(1);
            auditoria.FechaCreacionToken = DateTime.UtcNow;
            
            usuario.Auditoria = auditoria;
            await _usuarioRepo.UpdateAsync(usuario);

            var enlace = urlHelper.Action(
                action:     "Restablecer",
                controller: "RecuperacionContraseña",
                values:     new { token = auditoria.TokenRecuperacion },
                protocol:   "https");

            var cuerpo = $"Haz clic en el siguiente enlace para restablecer tu contraseña: " +
                         $"<a href='{enlace}'>Restablecer Contraseña</a>" +
                         $"<br/><small>El enlace expira en 1 hora.</small>";

            await _emailService.SendEmailAsync(correo, "Recuperación de Contraseña", cuerpo);
            return true;
        }

        public bool RestablecerContraseña(string token, string nuevaContraseña)
        {
            var usuario = _usuarioRepo.Query()
                .Include(u => u.Auditoria)
                .FirstOrDefault(u => u.Auditoria != null
                                  && u.Auditoria.TokenRecuperacion == token
                                  && u.Auditoria.TokenExpiracion > DateTime.UtcNow);

            if (usuario == null)
            {
                _logger.LogWarning("Intento de restablecimiento con token inválido o expirado.");
                return false;
            }

            usuario.Contraseña = _authService.ConvertirContraseña(nuevaContraseña);
            usuario.FechaActualizacion = DateTime.UtcNow;
            
            if (usuario.Auditoria != null)
            {
                usuario.Auditoria.TokenRecuperacion = null;
                usuario.Auditoria.TokenExpiracion = null;
                usuario.Auditoria.FechaActualizacion = DateTime.UtcNow;
            }

            _usuarioRepo.UpdateAsync(usuario).GetAwaiter().GetResult();
            return true;
        }
    }
}
