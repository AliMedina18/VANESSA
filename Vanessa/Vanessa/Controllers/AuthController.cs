using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUsuarioRepository  _usuarioRepo;
        private readonly IAuthService        _authService;
        private readonly IEmailService       _emailService;
        private readonly IConfiguration      _config;
        private readonly ApplicationDbContext _context;   // solo para leer Roles

        public AuthController(
            IUsuarioRepository usuarioRepo,
            IAuthService authService,
            IEmailService emailService,
            IConfiguration config,
            ApplicationDbContext context)
        {
            _usuarioRepo  = usuarioRepo;
            _authService  = authService;
            _emailService = emailService;
            _config       = config;
            _context      = context;
        }

        // GET /Auth/Login
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // POST /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(int documento, string contraseña, int rolId)
        {
            var usuario = await _usuarioRepo.GetByDocumentoAsync(documento);

            if (usuario == null || !_authService.VerificarContraseña(usuario.Contraseña, contraseña))
            {
                ModelState.AddModelError("", "Documento o contraseña incorrectos.");
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }

            if (usuario.RolId != rolId)
            {
                ModelState.AddModelError("", "El rol seleccionado no es válido para este usuario.");
                ViewBag.Roles = _context.Roles.ToList();
                return View();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new(ClaimTypes.Name,           usuario.Nombre),
                new(ClaimTypes.Role,           usuario.Rol?.Nombre ?? "Desconocido")
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30) });

            return RedirectToAction("Index", "Home");
        }

        // GET /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        // GET /Auth/Registro
        public IActionResult Registro()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewBag.Roles = _context.Roles.ToList();
            return View();
        }

        // POST /Auth/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (!_authService.EsContraseñaSegura(usuario.Contraseña))
            {
                ModelState.AddModelError("Contraseña",
                    "La contraseña debe tener al menos 8 caracteres, con mayúscula, minúscula, número y carácter especial.");
                ViewBag.Roles = _context.Roles.ToList();
                return View(usuario);
            }

            if (usuario.Contraseña != usuario.ConfirmarContraseña)
            {
                ModelState.AddModelError("ConfirmarContraseña", "La confirmación de contraseña no coincide.");
                ViewBag.Roles = _context.Roles.ToList();
                return View(usuario);
            }

            if (await _usuarioRepo.ExistsByDocumentoAsync(usuario.Documento))
            {
                ModelState.AddModelError("Documento", "Este documento ya está registrado.");
                ViewBag.Roles = _context.Roles.ToList();
                return View(usuario);
            }

            usuario.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
            usuario.RolId      = _context.Roles.FirstOrDefault(r => r.Nombre == "Cliente")?.Id ?? 1;

            await _usuarioRepo.AddAsync(usuario);
            await _usuarioRepo.AsignarPermisosClienteAsync(usuario.Id);

            var coordinadorEmail = _config["EmailSettings:CoordinadorEmail"] ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(coordinadorEmail))
            {
                await _emailService.SendEmailAsync(coordinadorEmail,
                    "Nuevo Registro de Usuario",
                    $"Nombre: {usuario.Nombre}<br/>Correo: {usuario.Correo}<br/>Documento: {usuario.Documento}");
            }

            TempData["Success"] = "Registro exitoso. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }
    }
}
