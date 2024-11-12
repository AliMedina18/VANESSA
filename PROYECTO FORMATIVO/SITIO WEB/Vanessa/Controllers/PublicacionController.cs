using Microsoft.AspNetCore.Mvc;
using Vanessa.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Vanessa.Data;
using System.Security.Claims;

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

        // Obtener todas las publicaciones
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
    }
}
