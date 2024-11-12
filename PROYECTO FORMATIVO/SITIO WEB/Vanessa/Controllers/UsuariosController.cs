using Vanessa.Models;
using Vanessa.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using Vanessa.Services;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.IO;


namespace Vanessa.Controllers
{
    [Authorize(Roles = "Coordinador")] // Solo accesible por Coordinadores
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        // Servicio de autenticación
        public UsuariosController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Cargar roles una sola vez
        private SelectList GetRolesSelectList(int? selectedRoleId)
        {
            return new SelectList(
                _context.Roles.Where(r => r.Nombre != "Coordinador" && r.Nombre != "Cliente"),
                "Id",
                "Nombre",
                selectedRoleId
            );
        }

        // Generar reporte PDF de usuarios activos
        public async Task<IActionResult> GenerarReporte()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).Where(u => u.Activo).ToListAsync();

            // Crear un nuevo documento PDF
            using (var memoryStream = new MemoryStream())
            {
                var pdfDocument = new PdfDocument();
                var page = pdfDocument.AddPage();
                var graphics = XGraphics.FromPdfPage(page);

                // Definir el estilo de la fuente
                var font = new XFont("Verdana", 12); // Sin especificar XFontStyle.Regular

                // Título del reporte
                graphics.DrawString("Reporte de Usuarios Activos", font, XBrushes.Black, 50, 40);

                // Establecer las posiciones para los datos
                int yPosition = 80;

                // Llenar los datos de los usuarios
                foreach (var usuario in usuarios)
                {
                    graphics.DrawString($"Nombre: {usuario.Nombre ?? "N/A"}", font, XBrushes.Black, 50, yPosition);
                    graphics.DrawString($"Documento: {(usuario.Documento != null ? usuario.Documento.ToString() : "N/A")}", font, XBrushes.Black, 50, yPosition + 20);
                    graphics.DrawString($"Correo: {usuario.Correo ?? "N/A"}", font, XBrushes.Black, 50, yPosition + 40);
                    graphics.DrawString($"Rol: {usuario.Rol.Nombre ?? "N/A"}", font, XBrushes.Black, 50, yPosition + 60);

                    yPosition += 80; // Aumentar la posición para el siguiente usuario
                }

                // Guardar el archivo PDF en el flujo de memoria
                pdfDocument.Save(memoryStream, false);

                // Regresar el archivo PDF generado como una descarga
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
            }
        }

        // GET: Usuarios Activos
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).Where(u => u.Activo).ToListAsync();
            return View(usuarios);
        }

        // GET: Usuarios Inactivos
        public async Task<IActionResult> Inactivos()
        {
            var usuariosInactivos = await _context.Usuarios.Include(u => u.Rol)
                .Where(u => !u.Activo)
                .ToListAsync();
            return View(usuariosInactivos);
        }

        // GET: Usuarios/CreateOrEdit
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            Usuario usuario = id.HasValue
                ? await _context.Usuarios.FindAsync(id)
                : new Usuario();

            if (id.HasValue && usuario == null)
            {
                return NotFound();
            }

            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int? id, [Bind("Id,Nombre,Documento,Correo,Contraseña,ConfirmarContraseña,RolId,Activo")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Validar si la contraseña es segura (si es nueva o actualizada)
                if (!string.IsNullOrEmpty(usuario.Contraseña) && !_authService.EsContraseñaSegura(usuario.Contraseña))
                {
                    ModelState.AddModelError("", "La contraseña no cumple con los requisitos de seguridad.");
                    ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                    return View(usuario);
                }

                if (id == null || id == 0) // Crear nuevo usuario
                {
                    // Hash la contraseña antes de guardarla
                    usuario.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                    usuario.Activo = true;
                    usuario.FechaInactivo = null;
                    _context.Add(usuario);
                }
                else // Editar usuario existente
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar los campos del usuario
                    usuarioExistente.Nombre = usuario.Nombre;
                    usuarioExistente.Documento = usuario.Documento;
                    usuarioExistente.Correo = usuario.Correo;
                    usuarioExistente.RolId = usuario.RolId;

                    // Si la contraseña fue modificada, generar un nuevo hash
                    if (!string.IsNullOrEmpty(usuario.Contraseña))
                    {
                        usuarioExistente.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                    }

                    _context.Update(usuarioExistente);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
            return View(usuario);
        }

        // Acción para Marcar Usuario como Inactivo
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;
            usuario.FechaInactivo = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Acción para Reactivar Usuario
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = true;
            usuario.FechaInactivo = null;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inactivos));
        }

        // Acción para Eliminar Usuario Definitivamente (Manual)
        public async Task<IActionResult> EliminarDefinitivamente(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inactivos));
        }

        // Método para Eliminar Usuarios Inactivos Automáticamente después de 30 días
        public async Task EliminarUsuariosInactivosAutomaticamente()
        {
            var fechaLimite = DateTime.Now.AddDays(-30);
            var usuariosParaEliminar = await _context.Usuarios
                .Where(u => !u.Activo && u.FechaInactivo <= fechaLimite)
                .ToListAsync();

            if (usuariosParaEliminar.Any())
            {
                _context.Usuarios.RemoveRange(usuariosParaEliminar);
                await _context.SaveChangesAsync();
            }
        }
    }
}
