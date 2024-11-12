using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vanessa.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Acción GET para el login
        public IActionResult Login()
        {
            // Verificar si el usuario ya está autenticado
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Cargar los roles para el dropdown
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // Acción POST para manejar el login
        [HttpPost]
        public async Task<IActionResult> Login(int documento, string contraseña, int rolId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Documento == documento);

            if (usuario == null || !VerificarContraseña(usuario, contraseña))
            {
                ModelState.AddModelError("", "Documento o contraseña incorrecta.");
                ViewBag.Roles = await _context.Roles.ToListAsync(); // Reenvía los roles a la vista para el dropdown
                return View();
            }

            if (usuario.RolId != rolId)
            {
                ModelState.AddModelError("", "El rol seleccionado no es válido para este usuario.");
                ViewBag.Roles = await _context.Roles.ToListAsync(); // Reenvía los roles a la vista
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) // Expiración de la cookie
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            return RedirectToAction("Index", "Home");
        }

        // Acción para cerrar sesión
        public async Task<IActionResult> Logout()
        {
            // Eliminar la sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Asegurarse de que no se guarde en caché la página de login y otras páginas protegidas
            HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            HttpContext.Response.Headers["Pragma"] = "no-cache";
            HttpContext.Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Auth");
        }

        // Acción GET para el registro de usuario
        public IActionResult Registro()
        {
            // Redirigir al home si el usuario ya está autenticado
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Cargar los roles para el dropdown en la vista de registro
            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // Acción POST para manejar el registro de usuario
        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            // Validación de la contraseña segura
            if (!EsContraseñaSegura(usuario.Contraseña))
            {
                ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 8 caracteres, incluyendo una letra mayúscula, una letra minúscula, un número y un carácter especial.");
                ViewBag.Roles = _context.Roles.ToList(); // Cargar los roles en caso de error
                return View(usuario);
            }

            // Validación de que las contraseñas coincidan
            if (usuario.Contraseña != usuario.ConfirmarContraseña)
            {
                ModelState.AddModelError("ConfirmarContraseña", "La confirmación de la contraseña no coincide con la contraseña.");
                ViewBag.Roles = _context.Roles.ToList(); // Cargar los roles en caso de error
                return View(usuario);
            }

            // Si el modelo es válido
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Documento == usuario.Documento);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Documento", "Este documento ya está registrado.");
                    ViewBag.Roles = _context.Roles.ToList(); // Cargar los roles en caso de error
                    return View(usuario);
                }

                // Convertir la contraseña a un hash seguro
                usuario.Contraseña = ConvertirContraseña(usuario.Contraseña);
                usuario.RolId = _context.Roles.FirstOrDefault(r => r.Nombre == "Cliente")?.Id ?? 1;

                // Guardar el usuario en la base de datos
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Asignar permisos a usuarios "Clientes" automáticamente
                if (usuario.RolId == _context.Roles.FirstOrDefault(r => r.Nombre == "Cliente")?.Id)
                {
                    await AsignarPermisosParaUsuario(usuario.Id);
                }

                return RedirectToAction("Login");
            }

            ViewBag.Roles = _context.Roles.ToList(); // Cargar los roles en caso de error
            return View(usuario);
        }

        // Método para verificar la contraseña
        private bool VerificarContraseña(Usuario usuario, string contraseña)
        {
            var parts = usuario.Contraseña.Split('.');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            var hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashedPassword;
        }

        // Método para convertir la contraseña a un hash seguro
        private string ConvertirContraseña(string contraseña)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        // Verificación de contraseña segura
        private bool EsContraseñaSegura(string contraseña)
        {
            return contraseña.Length >= 8 &&
                   contraseña.Any(char.IsDigit) &&
                   contraseña.Any(char.IsUpper) &&
                   contraseña.Any(char.IsLower) &&
                   contraseña.Any(ch => !char.IsLetterOrDigit(ch));
        }

        // Asignar permisos a usuarios nuevos
        private async Task AsignarPermisosParaUsuario(int usuarioId)
        {
            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
            if (rolCliente == null)
            {
                return;
            }

            var permisosCliente = await _context.Permisos
                .Where(p => p.Nombre.Contains("Cliente"))
                .ToListAsync();

            foreach (var permiso in permisosCliente)
            {
                _context.UsuarioPermisos.Add(new UsuarioPermiso
                {
                    UsuarioId = usuarioId,
                    PermisoId = permiso.Id
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
