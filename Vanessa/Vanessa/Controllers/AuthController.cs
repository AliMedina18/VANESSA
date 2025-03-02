using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Models;
using System.Security.Claims;
using Vanessa.Services; // Asegúrate de agregar la referencia a AuthService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace Vanessa.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly EmailService _emailService;

        // Constructor con inyección de dependencias para el servicio y el contexto
        public AuthController(ApplicationDbContext context, AuthService authService, EmailService emailService)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
        }

        // Acción GET para el login
        public IActionResult Login()
        {
            // Verificar si el usuario ya está autenticado
            if (User?.Identity?.IsAuthenticated == true)
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

            if (usuario == null || !_authService.VerificarContraseña(usuario.Contraseña, contraseña))
            {
                ModelState.AddModelError("", "Documento o contraseña incorrecta.");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View();
            }

            if (usuario.RolId != rolId)
            {
                ModelState.AddModelError("", "El rol seleccionado no es válido para este usuario.");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Desconocido")
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
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            // Cargar los roles para el dropdown en la vista de registro, evitando posibles referencias nulas
            ViewBag.Roles = _context.Roles?.ToList() ?? new List<Rol>();

            return View();
        }

        // Acción POST para manejar el registro de usuario
        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            // Validación de la contraseña segura
            if (!_authService.EsContraseñaSegura(usuario.Contraseña))
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
                usuario.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                usuario.RolId = _context.Roles.FirstOrDefault(r => r.Nombre == "Cliente")?.Id ?? 1;

                // Guardar el usuario en la base de datos
                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Asignar permisos a usuarios "Clientes" automáticamente
                if (usuario.RolId == _context.Roles.FirstOrDefault(r => r.Nombre == "Cliente")?.Id)
                {
                    await AsignarPermisosParaUsuario(usuario.Id);
                }

                // Enviar correo electrónico al correo coordinadorepicsoft18@gmail.com
                var subject = "Nuevo Registro de Usuario";
                var body = $"Se ha registrado un nuevo usuario con los siguientes datos:<br/>" +
                           $"Nombre: {usuario.Nombre}<br/>" +
                           $"Correo: {usuario.Correo}<br/>" +
                           $"Documento: {usuario.Documento}<br/>" +
                           $"Rol: {usuario.RolId}";  // Puedes modificar esto según la estructura de tus datos

                // Llamar al servicio de correo para enviar el email
                await _emailService.SendEmailAsync("coordinadorepicsoft18@gmail.com", subject, body);

                // Redirigir a la página de registro o a alguna otra página después del registro
                return RedirectToAction("Registro");
            }

            ViewBag.Roles = _context.Roles.ToList(); // Cargar los roles en caso de error
            return View(usuario);
        }
        // Método para asignar permisos a usuarios nuevos
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



