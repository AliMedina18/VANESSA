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
using System;

namespace Vanessa.Controllers
{
    public class SemilleroController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _context;

        public SemilleroController(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, ApplicationDbContext context)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _context = context;
        }

        // Obtener todos los semilleros
        public IActionResult Index()
        {
            var semilleros = _context.Semilleros.ToList(); // Usamos EF para obtener todos los semilleros
            return View(semilleros);
        }

        // Crear semillero
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Semillero semillero, IFormFile imagen)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;

                // Obtener el ID del usuario autenticado
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Este es el ID del usuario autenticado
                semillero.UsuarioId = int.Parse(usuarioId); // Asegúrate de que se almacene como un entero

                // Subir la imagen
                if (imagen != null && imagen.Length > 0)
                {
                    fileName = Path.GetFileName(imagen.FileName); // Obtener el nombre del archivo
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", fileName); // Ruta donde se guardará la imagen

                    // Crear la carpeta si no existe
                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Guardar la imagen en la carpeta
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                }

                // Insertar en la base de datos
                semillero.Imagen = fileName ?? string.Empty; // Solo se guarda el nombre de la imagen
                _context.Semilleros.Add(semillero);
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos

                return RedirectToAction(nameof(Index));
            }

            return View(semillero);
        }

        // Editar semillero
        public IActionResult Edit(int id)
        {
            var semillero = _context.Semilleros.Find(id);
            if (semillero == null)
            {
                return NotFound();
            }
            return View(semillero);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Semillero semillero, IFormFile imagen)
        {
            if (id != semillero.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string fileName = null;

                // Subir nueva imagen si se proporciona
                if (imagen != null && imagen.Length > 0)
                {
                    fileName = Path.GetFileName(imagen.FileName);
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", fileName);

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                }
                else
                {
                    fileName = semillero.Imagen; // Si no se proporciona imagen, mantener la existente
                }

                // Actualizar en la base de datos
                semillero.Imagen = fileName ?? semillero.Imagen; // Usar el nombre de imagen nuevo o el antiguo
                _context.Semilleros.Update(semillero);
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos

                return RedirectToAction(nameof(Index));
            }

            return View(semillero);
        }

        // Eliminar semillero - Acción para eliminar directamente sin confirmación
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Buscar el semillero por id (usando FindAsync para ser más eficiente en operaciones asincrónicas)
                var semillero = await _context.Semilleros.FindAsync(id);

                if (semillero == null)
                {
                    return NotFound();
                }

                // Eliminar imagen del servidor si existe
                if (!string.IsNullOrEmpty(semillero.Imagen))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", semillero.Imagen);

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

                // Eliminar el semillero de la base de datos
                _context.Semilleros.Remove(semillero);
                await _context.SaveChangesAsync(); // Guardar cambios en la base de datos de manera asincrónica

                return RedirectToAction(nameof(Index)); // Redirigir a la vista principal después de la eliminación
            }
            catch (Exception ex)
            {
                // Manejo de excepciones para registrar cualquier error en los logs del servidor
                return StatusCode(500, $"Error al eliminar el semillero: {ex.Message}");
            }
        }
    }
}
