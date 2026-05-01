using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly IPublicacionRepository _publicacionRepo;
        private readonly IWebHostEnvironment    _env;
        private readonly ILogger<PublicacionController> _logger;

        private string UploadsPath => Path.Combine(_env.WebRootPath, "uploads");

        public PublicacionController(
            IPublicacionRepository publicacionRepo,
            IWebHostEnvironment env,
            ILogger<PublicacionController> logger)
        {
            _publicacionRepo = publicacionRepo;
            _env    = env;
            _logger = logger;
            var up = Path.Combine(env.WebRootPath, "uploads");
            if (!Directory.Exists(up)) Directory.CreateDirectory(up);
        }

        [Authorize(Roles = "Docente,Estudiante,Coordinador")]
        public async Task<IActionResult> MuroGeneral(string? searchString, string? category, DateTime? dateFilter)
        {
            ViewData["SearchString"] = searchString;
            ViewData["Category"]     = category;
            ViewData["DateFilter"]   = dateFilter?.ToString("yyyy-MM-dd");

            var query = _publicacionRepo.QueryWithUsuario();

            if (!string.IsNullOrEmpty(searchString))
            {
                var s = searchString.ToLower().Trim();
                query = query.Where(p =>
                    (p.Titulo != null && p.Titulo.ToLower().Contains(s)) ||
                    (p.Contenido != null && p.Contenido.ToLower().Contains(s)));
            }
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.TipoPublicacion != null && p.TipoPublicacion.Nombre == category);
            }
            if (dateFilter.HasValue)
                query = query.Where(p => p.FechaPublicacion.Date == dateFilter.Value.Date);

            return View(await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query));
        }

        [Authorize(Roles = "Docente,Estudiante,Coordinador")]
        public async Task<IActionResult> MiMuro(string? searchString, string? category, DateTime? dateFilter)
        {
            ViewData["SearchString"] = searchString;
            ViewData["Category"]     = category;
            ViewData["DateFilter"]   = dateFilter?.ToString("yyyy-MM-dd");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            var pubs = await _publicacionRepo.GetByUsuarioIdAsync(userId);
            IEnumerable<Publicacion> result = pubs;

            if (!string.IsNullOrEmpty(searchString))
            {
                var s = searchString.ToLower().Trim();
                result = result.Where(p =>
                    (p.Titulo != null && p.Titulo.ToLower().Contains(s)) ||
                    (p.Contenido != null && p.Contenido.ToLower().Contains(s)));
            }
            if (!string.IsNullOrEmpty(category))
            {
                result = result.Where(p => p.TipoPublicacion != null && p.TipoPublicacion.Nombre == category);
            }
            if (dateFilter.HasValue)
                result = result.Where(p => p.FechaPublicacion.Date == dateFilter.Value.Date);

            return View(result.ToList());
        }

        [Authorize(Roles = "Docente,Estudiante,Coordinador")]
        public IActionResult Crear() => View();

        [HttpPost]
        [Authorize(Roles = "Docente,Estudiante,Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            Publicacion publicacion,
            IFormFile? ImagenFile,
            List<IFormFile>? ArchivosAdjuntos)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            if (ImagenFile == null || ImagenFile.Length == 0)
            {
                ModelState.AddModelError("ImagenFile", "La imagen es obligatoria.");
                return View(publicacion);
            }

            publicacion.UsuarioId        = userId;
            publicacion.FechaPublicacion = DateTime.UtcNow;
            

            try
            {
                var imgName = Path.GetFileName(ImagenFile.FileName);
                using (var s = new FileStream(Path.Combine(UploadsPath, imgName), FileMode.Create))
                    await ImagenFile.CopyToAsync(s);
                publicacion.Imagen = "/uploads/" + imgName;

                if (ArchivosAdjuntos?.Count > 0)
                {
                    publicacion.Adjuntos = publicacion.Adjuntos ?? new List<PublicacionAdjunto>();
                    int orden = 1;
                    foreach (var archivo in ArchivosAdjuntos)
                    {
                        var fn = Path.GetFileName(archivo.FileName);
                        using var s = new FileStream(Path.Combine(UploadsPath, fn), FileMode.Create);
                        await archivo.CopyToAsync(s);
                        publicacion.Adjuntos.Add(new PublicacionAdjunto 
                        { 
                            RutaArchivo = "/uploads/" + fn,
                            Orden = orden++
                        });
                    }
                }

                await _publicacionRepo.AddAsync(publicacion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la publicación.");
                ModelState.AddModelError("", "Hubo un error al guardar la publicación.");
                return View(publicacion);
            }

            return RedirectToAction(nameof(MiMuro));
        }

        [Authorize]
        public async Task<IActionResult> Editar(int id)
        {
            var pub = await _publicacionRepo.GetByIdAsync(id);
            if (pub == null) return NotFound();
            return View(pub);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var pub = await _publicacionRepo.GetByIdAsync(id);
            if (pub == null)
                return Json(new { success = false, message = "Publicación no encontrada." });

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
                return Unauthorized();

            if (!User.IsInRole("Coordinador") && pub.UsuarioId != userId)
                return Json(new { success = false, message = "No tienes permisos para eliminar esta publicación." });

            await _publicacionRepo.DeleteAsync(pub);
            return Json(new { success = true, message = "Publicación eliminada correctamente." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPostData(int id)
        {
            var pub = await _publicacionRepo.GetByIdWithUsuarioAsync(id);
            if (pub == null) return NotFound();

            var actividades = string.IsNullOrEmpty(pub.ActividadesPublicacion)
                ? new List<object>()
                : pub.ActividadesPublicacion.Split(',')
                    .Select(a => (object)new { url = a.Trim(), nombre = Path.GetFileName(a.Trim()) })
                    .ToList();

            return Json(new
            {
                nombrePublicacion    = pub.NombrePublicacion,
                imagenPublicacion    = !string.IsNullOrEmpty(pub.ImagenPublicacion) ? pub.ImagenPublicacion : "/img/default.jpg",
                contenidoPublicacion = pub.ContenidoPublicacion ?? "Sin contenido",
                tipoPublicacion      = pub.TipoPublicacion?.Nombre ?? "No especificado",
                lugarPublicacion     = pub.LugarPublicacion     ?? "No especificado",
                fechaPublicacion     = pub.FechaPublicacion.ToString("yyyy-MM-dd"),
                horaPublicacion      = pub.FechaPublicacion.ToString("HH:mm"),
                usuarioPublicacion   = pub.Usuario?.Nombre ?? "Anónimo",
                actividadesPublicacion = actividades
            });
        }
    }
}
