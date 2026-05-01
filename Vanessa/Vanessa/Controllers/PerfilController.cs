using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanessa.Interfaces;
using Vanessa.Models;
using Vanessa.Models.ViewModels;

namespace Vanessa.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IAuthService       _authService;
        private readonly IEmailService      _emailService;
        private readonly IConfiguration     _config;

        public PerfilController(
            IUsuarioRepository usuarioRepo,
            IAuthService authService,
            IEmailService emailService,
            IConfiguration config)
        {
            _usuarioRepo  = usuarioRepo;
            _authService  = authService;
            _emailService = emailService;
            _config       = config;
        }

        public async Task<IActionResult> VerPerfil()
        {
            var usuario = await _usuarioRepo.GetByIdAsync(ObtenerUsuarioId());
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        public async Task<IActionResult> EditarPerfil()
        {
            var usuario = await _usuarioRepo.GetByIdAsync(ObtenerUsuarioId());
            if (usuario == null) return NotFound();
            return View(new EditarPerfilViewModel
            {
                Id     = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var usuario = await _usuarioRepo.GetByIdAsync(ObtenerUsuarioId());
            if (usuario == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(vm.Contraseña))
            {
                if (vm.Contraseña != vm.ConfirmarContraseña)
                {
                    ModelState.AddModelError("ConfirmarContraseña", "La confirmación de contraseña no coincide.");
                    return View(vm);
                }
                usuario.Contraseña = _authService.ConvertirContraseña(vm.Contraseña);
            }

            usuario.Nombre = vm.Nombre;
            usuario.Correo = vm.Correo;
            await _usuarioRepo.UpdateAsync(usuario);

            var coord = _config["EmailSettings:CoordinadorEmail"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(coord))
                await _emailService.SendEmailAsync(coord, "Perfil Editado",
                    $"El usuario {usuario.Nombre} (ID:{usuario.Id}) actualizó su perfil.");

            TempData["Success"] = "Perfil actualizado correctamente.";
            return RedirectToAction("VerPerfil");
        }

        public async Task<IActionResult> EliminarPerfil()
        {
            var userId  = ObtenerUsuarioId();
            var usuario = await _usuarioRepo.GetByIdAsync(userId);
            if (usuario == null) return NotFound();

            bool tienePermiso = _usuarioRepo.Query()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UsuarioPermisos!)
                .Any(up => up.Permiso.Nombre == "EliminarPerfil");

            if (!tienePermiso)
            {
                TempData["Error"] = "No tienes permiso para eliminar tu perfil.";
                return RedirectToAction("VerPerfil");
            }
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPerfilConfirmed()
        {
            var userId  = ObtenerUsuarioId();
            var usuario = await _usuarioRepo.GetByIdAsync(userId);
            if (usuario == null) return NotFound();

            var coord = _config["EmailSettings:CoordinadorEmail"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(coord))
                await _emailService.SendEmailAsync(coord, "Perfil Eliminado",
                    $"El usuario {usuario.Nombre} (ID:{usuario.Id}) eliminó su perfil.");

            await _usuarioRepo.DeleteAsync(usuario);
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }

        private int ObtenerUsuarioId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }
}
