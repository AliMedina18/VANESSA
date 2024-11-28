using Microsoft.AspNetCore.Mvc;
using Vanessa.Models;
using Vanessa.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Vanessa.Controllers
{
    public class PublicacionController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;

        public PublicacionController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        // Obtener todas las publicaciones con funcionalidad de búsqueda
        public IActionResult Index()
        {
            var publicaciones = _context.Publicaciones.ToList();
            return View(publicaciones);
        }

        // Crear nueva publicación
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Publicacion publicacion, IFormFile imagen)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;

                // Obtener el ID del usuario autenticado
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                publicacion.UsuarioId = int.Parse(usuarioId);
                // Subir la imagen
                if (imagen != null && imagen.Length > 0)
                {
                    // Obtener el nombre del archivo
                    fileName = Path.GetFileName(imagen.FileName);
                    // Ruta para guardar la imagen
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", fileName);

                    // Crear directorio si no existe
                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Guardar la imagen en la carpeta 'images'
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                }

                // Asignar el nombre de la imagen a la propiedad de la publicación
                publicacion.ImagenPublicacion = fileName ?? string.Empty;

                // Guardar la publicación en la base de datos
                _context.Publicaciones.Add(publicacion);
                await _context.SaveChangesAsync();

                // Redirigir a la vista principal
                return RedirectToAction(nameof(Index));
            }

            return View(publicacion);
        }

        //Get editasr publicación
        public IActionResult Edit(int id)
        {
            var publicacion = _context.Publicaciones.Find(id);
            if (publicacion == null)
            {
                return NotFound();
            }
            return View(publicacion);
        }

        //Post editar Publicación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publicacion publicacion, IFormFile? imagen, string ImagenActual)
        {
            if (id != publicacion.Id_Publicacion)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    //Manejar la imagen y ajuste
                    if (imagen != null && imagen.Length > 0)
                    {
                        string fileName = Path.GetFileName(imagen.FileName);
                        string directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

                        // Crear carpeta si no existe
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        string filePath = Path.Combine(directoryPath, fileName);

                        // Guardar nueva imagen
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imagen.CopyToAsync(stream);
                        }

                        publicacion.ImagenPublicacion = fileName; // Actualizar con nueva imagen
                    }
                    else
                    {
                        publicacion.ImagenPublicacion = ImagenActual; // Mantener la imagen existente
                    }

                    // Actualizar publicaciones de la base de datos
                    _context.Publicaciones.Update(publicacion);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if(!_context.Publicaciones.Any(e=> e.Id_Publicacion == publicacion.Id_Publicacion))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(publicacion);
        }

        //Eliminar Publicación 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Buscar el publicación por id (usando FindAsync para ser más eficiente en operaciones asincrónicas)
                var publicacion = await _context.Publicaciones.FindAsync(id);

                if (publicacion == null)
                {
                    return NotFound();
                }

                // Eliminar imagen del servidor si existe
                if (!string.IsNullOrEmpty(publicacion.ImagenPublicacion))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", publicacion.ImagenPublicacion);

                    // Verificar si la imagen existe en la ruta especificada
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath); // Eliminar archivo de imagen
                    }
                    else
                    {
                        // Si el archivo no existe, puedes registrar un mensaje de error o advertencia en los logs
                        Console.WriteLine($"No se pudo encontrar la imagen en la ruta: {filePath}");
                    }
                }
                // Eliminar la publicacion de la base de datos
                _context.Publicaciones.Remove(publicacion);
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos de manera asincrónica

                return RedirectToAction(nameof(Index)); // Redirigir a la vista principal después de la eliminación
            }
            catch (Exception ex)
            {
                // Manejo de excepciones para registrar cualquier error en los logs del servidor
                return StatusCode(500, $"Error al eliminar la publicación: {ex.Message}");
            }
        }
    }
}
