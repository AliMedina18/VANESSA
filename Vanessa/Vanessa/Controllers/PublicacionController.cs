using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Vanessa.Models;
using Vanessa.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;

namespace Vanessa.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PublicacionController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PublicacionController(ApplicationDbContext context, ILogger<PublicacionController> logger, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        private void EnsureUploadsDirectoryExists()
        {
            var uploadsPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
        }

        // GET: MuroGeneral (muestra todas las publicaciones)
        public async Task<IActionResult> MuroGeneral(string searchString, string category, DateTime? dateFilter)
        {
            // Restringir acceso a Docentes, Estudiantes y Coordinadores
            if (!User.IsInRole("Docente") && !User.IsInRole("Estudiante") && !User.IsInRole("Coordinador"))
            {
                return Unauthorized();
            }

            ViewData["SearchString"] = searchString;
            ViewData["Category"] = category;
            ViewData["DateFilter"] = dateFilter?.ToString("yyyy-MM-dd");

            var publicaciones = _context.Publicaciones.Include(p => p.Usuario).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower().Trim();
                publicaciones = publicaciones.Where(p =>
                    (!string.IsNullOrEmpty(p.NombrePublicacion) && p.NombrePublicacion.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(p.ContenidoPublicacion) && p.ContenidoPublicacion.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(category))
            {
                publicaciones = publicaciones.Where(p => p.TipoPublicacion == category);
            }

            if (dateFilter.HasValue)
            {
                publicaciones = publicaciones.Where(p => p.FechaPublicacion.Date == dateFilter.Value.Date);
            }

            return View(await publicaciones.ToListAsync());
        }

        // GET: MiMuro (muestra solo las publicaciones del usuario actual)
        public async Task<IActionResult> MiMuro(string searchString, string category, DateTime? dateFilter)
        {
            // Restringir acceso solo a Docentes, Estudiantes y Coordinadores
            if (!User.IsInRole("Docente") && !User.IsInRole("Estudiante") && !User.IsInRole("Coordinador"))
            {
                return Unauthorized();
            }

            ViewData["SearchString"] = searchString;
            ViewData["Category"] = category;
            ViewData["DateFilter"] = dateFilter?.ToString("yyyy-MM-dd");

            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                _logger.LogWarning("UsuarioId no encontrado en la sesión");
                return Unauthorized();
            }

            var misPublicaciones = _context.Publicaciones.Where(p => p.UsuarioId == usuarioId);

            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower().Trim();
                misPublicaciones = misPublicaciones.Where(p =>
                    (!string.IsNullOrEmpty(p.NombrePublicacion) && p.NombrePublicacion.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(p.ContenidoPublicacion) && p.ContenidoPublicacion.ToLower().Contains(searchLower))
                );
            }

            if (!string.IsNullOrEmpty(category))
            {
                misPublicaciones = misPublicaciones.Where(p => p.TipoPublicacion == category);
            }

            if (dateFilter.HasValue)
            {
                misPublicaciones = misPublicaciones.Where(p => p.FechaPublicacion.Date == dateFilter.Value.Date);
            }

            return View(await misPublicaciones.ToListAsync());
        }

        // GET: Crear
        public IActionResult Crear()
        {
            if (!User.IsInRole("Docente") && !User.IsInRole("Estudiante") && !User.IsInRole("Coordinador"))
            {
                return Unauthorized(); // Restringir acceso si no es Docente, Estudiante o Coordinador
            }

            return View();
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Publicacion publicacion, IFormFile? ImagenFile, List<IFormFile>? ArchivosAdjuntos)
        {
            _logger.LogInformation("🚀 Intentando crear publicación...");

            // Verificar si el usuario tiene permisos para crear publicaciones
            if (!User.IsInRole("Docente") && !User.IsInRole("Estudiante") && !User.IsInRole("Coordinador"))
            {
                return Unauthorized();
            }

            // Obtener ID del usuario autenticado
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                _logger.LogWarning("⚠️ UsuarioId no encontrado en la sesión");
                return Unauthorized();
            }

            publicacion.UsuarioId = usuarioId;
            publicacion.FechaPublicacion = DateTime.Now;
            publicacion.HoraPublicacion = DateTime.Now.TimeOfDay;

            // Verificar si la imagen es obligatoria
            if (ImagenFile == null || ImagenFile.Length == 0)
            {
                _logger.LogWarning("❌ Imagen requerida para la publicación");
                ModelState.AddModelError("ImagenFile", "La imagen es obligatoria.");
                return View(publicacion);
            }

            EnsureUploadsDirectoryExists();

            try
            {
                // Guardar imagen de la publicación
                var fileName = Path.GetFileName(ImagenFile.FileName);
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImagenFile.CopyToAsync(stream);
                }
                publicacion.ImagenPublicacion = "/uploads/" + fileName;
                _logger.LogInformation("📸 Imagen guardada correctamente: {ImagenPublicacion}", publicacion.ImagenPublicacion);

                // Guardar archivos adjuntos si existen
                if (ArchivosAdjuntos != null && ArchivosAdjuntos.Count > 0)
                {
                    var adjuntos = new List<string>();
                    foreach (var archivo in ArchivosAdjuntos)
                    {
                        var adjuntoFileName = Path.GetFileName(archivo.FileName);
                        var adjuntoFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", adjuntoFileName);

                        using (var stream = new FileStream(adjuntoFilePath, FileMode.Create))
                        {
                            await archivo.CopyToAsync(stream);
                        }
                        adjuntos.Add("/uploads/" + adjuntoFileName);
                    }
                    publicacion.ActividadesPublicacion = string.Join(",", adjuntos);
                    _logger.LogInformation("📎 Archivos adjuntos guardados correctamente: {ActividadesPublicacion}", publicacion.ActividadesPublicacion);
                }

                // Guardar publicación en la base de datos
                _context.Add(publicacion);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Publicación creada con éxito: {@Publicacion}", publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al guardar la publicación en la base de datos");
                ModelState.AddModelError("", "Hubo un error al guardar la publicación.");
                return View(publicacion);
            }

            _logger.LogInformation("🔄 Redirigiendo a MiMuro...");
            return RedirectToAction(nameof(MiMuro));
        }

        // GET: Editar
        public async Task<IActionResult> Editar(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(); // Solo usuarios autenticados pueden acceder
            }

            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null)
            {
                return NotFound("No se encontró la publicación.");
            }

            // 🔹 Permitir que todos los usuarios autenticados vean la vista de edición
            return View(publicacion);
        }

        // POST: Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null)
            {
                return Json(new { success = false, message = "Publicación no encontrada." });
            }

            // Obtener ID del usuario autenticado
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null || !int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized();
            }

            // Verificar permisos: Coordinador puede eliminar todo, Docentes y Estudiantes necesitan autorización
            if (User.IsInRole("Docente") || User.IsInRole("Estudiante"))
            {
                return Json(new { success = false, message = "Necesitas autorización del Coordinador para eliminar publicaciones." });
            }

            if (User.IsInRole("Coordinador") || publicacion.UsuarioId == usuarioId)
            {
                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Publicación eliminada correctamente." });
            }

            return Json(new { success = false, message = "No tienes permisos para eliminar esta publicación." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPostData(int id)
        {
            var publicacion = await _context.Publicaciones
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id_Publicacion == id);

            if (publicacion == null)
            {
                return NotFound();
            }

            // Manejo seguro de la lista de actividades (archivos adjuntos)
            var actividades = new List<object>();
            if (!string.IsNullOrEmpty(publicacion.ActividadesPublicacion))
            {
                actividades = publicacion.ActividadesPublicacion
                    .Split(',')
                    .Select(a => new { url = a.Trim(), nombre = Path.GetFileName(a.Trim()) })
                    .ToList<object>(); // Convertimos a List<object> para evitar errores
            }

            var response = new
            {
                nombrePublicacion = publicacion.NombrePublicacion,
                imagenPublicacion = !string.IsNullOrEmpty(publicacion.ImagenPublicacion) ? publicacion.ImagenPublicacion : "/img/default.jpg",
                contenidoPublicacion = publicacion.ContenidoPublicacion ?? "Sin contenido",
                tipoPublicacion = publicacion.TipoPublicacion ?? "No especificado",
                lugarPublicacion = publicacion.LugarPublicacion ?? "No especificado",
                fechaPublicacion = publicacion.FechaPublicacion.ToString("yyyy-MM-dd"),
                horaPublicacion = publicacion.HoraPublicacion.ToString(@"hh\:mm"),
                usuarioPublicacion = publicacion.Usuario?.Nombre ?? "Anónimo",
                actividadesPublicacion = actividades
            };

            return Json(response);
        }

    }
}
