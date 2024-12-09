using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PublicacionController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // Listar publicaciones
        public async Task<IActionResult> Index()
        {
            var publicaciones = await _context.Publicaciones.ToListAsync();
            return View(publicaciones);
        }

        // Crear una nueva publicación - Vista de formulario
        public IActionResult Crear()
        {
            return View();
        }

        // Crear una nueva publicación - Procesar formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Publicacion publicacion)
        {
            if (!ModelState.IsValid)
            {
                return View(publicacion);
            }

            try
            {
                if (publicacion.ImagenArchivo != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "imagenes");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + publicacion.ImagenArchivo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await publicacion.ImagenArchivo.CopyToAsync(fileStream);
                    }
                    publicacion.ImagenPublicacion = $"/imagenes/{uniqueFileName}";
                }

                _context.Publicaciones.Add(publicacion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear publicación: {ex.Message}");
                ModelState.AddModelError("", "Ocurrió un error al crear la publicación");
                return View(publicacion);
            }
        }

        // Editar publicación - Vista de formulario
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion == null) return NotFound();
            return View(publicacion);
        }

        // Editar publicación - Procesar formulario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Publicacion publicacion)
        {
            if (id != publicacion.Id_Publicacion) return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(publicacion);
            }

            try
            {
                var publicacionExistente = await _context.Publicaciones.FindAsync(publicacion.Id_Publicacion);
                if (publicacionExistente == null) return NotFound();

                publicacionExistente.NombrePublicacion = publicacion.NombrePublicacion;
                publicacionExistente.ContenidoPublicacion = publicacion.ContenidoPublicacion;
                publicacionExistente.TipoPublicacion = publicacion.TipoPublicacion;
                publicacionExistente.LugarPublicacion = publicacion.LugarPublicacion;
                publicacionExistente.FechaPublicacion = publicacion.FechaPublicacion;
                publicacionExistente.HoraPublicacion = publicacion.HoraPublicacion;
                publicacionExistente.ActividadesPublicacion = publicacion.ActividadesPublicacion;

                if (publicacion.ImagenArchivo != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "imagenes");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + publicacion.ImagenArchivo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await publicacion.ImagenArchivo.CopyToAsync(fileStream);
                    }
                    publicacionExistente.ImagenPublicacion = $"/imagenes/{uniqueFileName}";
                }

                _context.Update(publicacionExistente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PublicacionExists(publicacion.Id_Publicacion)) return NotFound();
                else throw;
            }
        }

        // Eliminar publicación - Vista de confirmación
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var publicacion = await _context.Publicaciones
                .FirstOrDefaultAsync(m => m.Id_Publicacion == id);
            if (publicacion == null) return NotFound();
            return View(publicacion);
        }

        // Eliminar publicación - Procesar confirmación
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int id)
        {
            var publicacion = await _context.Publicaciones.FindAsync(id);
            if (publicacion != null)
            {
                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(publicacion.ImagenPublicacion))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, publicacion.ImagenPublicacion.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PublicacionExists(int? id)
        {
            return _context.Publicaciones.Any(e => e.Id_Publicacion == id);
        }
    }
}
