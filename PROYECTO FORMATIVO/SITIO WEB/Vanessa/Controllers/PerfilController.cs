using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Models;
using Vanessa.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;

namespace Vanessa.Controllers
{
    [Authorize] // Aseguramos que solo los usuarios autenticados puedan acceder
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        // Constructor que recibe el contexto y el servicio de autenticación
        public PerfilController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Acción GET para ver el perfil (permitido solo para el propio perfil del usuario)
        public async Task<IActionResult> VerPerfil()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // Acción GET para editar el perfil (permitido solo al usuario dueño del perfil)
        public async Task<IActionResult> EditarPerfil()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            // No permitir editar el rol
            ViewBag.Roles = _context.Roles.Where(r => r.Id != usuario.RolId).ToList();

            return View(usuario);
        }

        // Acción POST para actualizar el perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(Usuario usuarioActualizado)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificación de la confirmación de la contraseña
            if (!string.IsNullOrEmpty(usuarioActualizado.Contraseña) && usuarioActualizado.Contraseña != usuarioActualizado.ConfirmarContraseña)
            {
                ModelState.AddModelError("ConfirmarContraseña", "La confirmación de la contraseña no coincide.");
                return View(usuario);
            }

            // Si la contraseña fue cambiada, actualízala
            if (!string.IsNullOrEmpty(usuarioActualizado.Contraseña))
            {
                usuario.Contraseña = _authService.ConvertirContraseña(usuarioActualizado.Contraseña);
            }

            // Actualizar los campos de nombre y correo
            usuario.Nombre = usuarioActualizado.Nombre;
            usuario.Correo = usuarioActualizado.Correo;

            // Guardar los cambios en la base de datos
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerPerfil");
        }

        // Acción GET para eliminar el perfil
        public async Task<IActionResult> EliminarPerfil()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar si el usuario tiene el permiso "EliminarPerfil"
            var tienePermisoEliminarPerfil = await _context.UsuarioPermisos
                .AnyAsync(up => up.UsuarioId == usuario.Id && up.Permiso.Nombre == "EliminarPerfil");

            if (tienePermisoEliminarPerfil)
            {
                return View(usuario);
            }

            // Si no tiene permiso, informamos que debe ser asignado por un Coordinador
            TempData["Error"] = "No tienes permiso para eliminar tu perfil. El permiso debe ser otorgado por un Coordinador.";
            return RedirectToAction("VerPerfil");
        }

        // Acción POST para eliminar el perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarPerfilConfirmed()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar si el usuario tiene el permiso "EliminarPerfil"
            var tienePermisoEliminarPerfil = await _context.UsuarioPermisos
                .AnyAsync(up => up.UsuarioId == usuario.Id && up.Permiso.Nombre == "EliminarPerfil");

            if (!tienePermisoEliminarPerfil)
            {
                TempData["Error"] = "No tienes permiso para eliminar tu perfil. El permiso debe ser otorgado por un Coordinador.";
                return RedirectToAction("VerPerfil");
            }

            // Eliminar el usuario de la base de datos
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            // Cerrar sesión después de eliminar el perfil
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }
    }
}
