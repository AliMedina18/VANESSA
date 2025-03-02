using Microsoft.AspNetCore.Authorization;
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
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Vanessa.Controllers
{
    [Authorize] // Asegura que solo usuarios autenticados puedan acceder al controlador
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

        // 🔹 SOLO ESTUDIANTES, DOCENTES Y COORDINADORES PUEDEN VER SEMILLEROS
        [Authorize(Roles = "Estudiante,Docente,Coordinador")]
        public IActionResult Index()
        {
            if (TempData["ErrorPermiso"] != null)
            {
                ViewBag.ErrorPermiso = TempData["ErrorPermiso"]; // Pasamos el mensaje a la vista
            }

            var semilleros = _context.Semilleros.ToList(); // Obtener todos los semilleros de la base de datos
            return View(semilleros);
        }


        // Generar PDF con el listado de semilleros
        [Obsolete]
        [Authorize(Roles = "Docente,Coordinador")]
        public IActionResult GenerarPdf()
        {
            var semilleros = _context.Semilleros.ToList(); // Obtener los semilleros de la base de datos

            var pdfDocument = new PdfDocument();
            var fontTitle = new XFont("Verdana Bold", 16);
            var fontSubTitle = new XFont("Verdana", 12);
            var fontContent = new XFont("Verdana", 10);
            var fontPageNumber = new XFont("Verdana Bold", 8);
            var fontFooter = new XFont("Verdana Italic", 8);
            var fontHeader = new XFont("Verdana", 10);

            int pageNumber = 1;
            PdfPage page = pdfDocument.AddPage();
            XGraphics graphics = XGraphics.FromPdfPage(page);

            // Colores
            XColor lightBlue = XColor.FromArgb(220, 240, 255);  // Azul claro para el borde
            XColor darkBlue = XColor.FromArgb(60, 90, 150);     // Azul oscuro para los textos
            XColor gray = XColor.FromArgb(200, 200, 200);       // Gris para otras líneas

            // Títulos
            DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);

            double yPosition = 100; // Inicio del contenido
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            // Línea divisoria en azul oscuro
            graphics.DrawLine(new XPen(darkBlue, 1), leftMargin, yPosition - 10, rightMargin, yPosition - 10);
            yPosition += 20;

            graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue), new XRect(page.Width - 190, 20, 150, 20), XStringFormats.TopRight);

            foreach (var semillero in semilleros)
            {
                if (yPosition > page.Height - 100) // Crear nueva página si se llena
                {
                    // Crear nueva página
                    page = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    // Volver a dibujar encabezado en la nueva página
                    DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);

                    // Nombre de la empresa en la esquina superior derecha
                    graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue), new XRect(page.Width - 150, 20, 150, 20), XStringFormats.TopRight);

                    yPosition = 100; // Restablecer la posición vertical
                }

                // Dibujar cuadro estilizado para cada semillero con solo línea azul claro alrededor
                graphics.DrawRoundedRectangle(new XPen(lightBlue, 2), leftMargin - 10, yPosition - 15, page.Width - 2 * leftMargin + 10, 85, 10, 10);

                // Dibujar contenido con texto en color azul oscuro
                graphics.DrawString($"Nombre: {semillero.Nombre ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
                graphics.DrawString($"Descripción: {semillero.Descripcion ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 20);

                yPosition += 100; // Incrementar para el siguiente semillero
            }

            // Número de página en la parte superior derecha (Estilo APA)
            graphics.DrawString($"{pageNumber}", fontPageNumber, new XSolidBrush(darkBlue), new XRect(page.Width - 40, 20, 30, 20), XStringFormats.TopRight);

            // Pie de página con la fecha de creación del PDF
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            // Incrementar el contador de páginas
            pageNumber++;

            // Guardar el PDF en un MemoryStream
            var stream = new MemoryStream();
            pdfDocument.Save(stream, false);
            stream.Position = 0;

            // Retornar el archivo PDF como una respuesta para descarga
            return File(stream, "application/pdf", "Semilleros.pdf");
        }

        // Método para dibujar el título y subtítulo
        private void DrawTitle(XGraphics graphics, PdfPage page, XFont fontTitle, XFont fontSubTitle, XColor darkBlue)
        {
            graphics.DrawString("Listado de Semilleros", fontTitle, new XSolidBrush(darkBlue),
                new XRect(0, 30, page.Width.Point, 40), XStringFormats.TopCenter);

            graphics.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                fontSubTitle, new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, 60, page.Width.Point, 20), XStringFormats.TopCenter);
        }

        // 🔹 SOLO DOCENTES Y COORDINADORES PUEDEN CREAR SEMILLEROS
        [Authorize(Roles = "Docente,Coordinador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Docente,Coordinador")]
        public async Task<IActionResult> Create(Semillero semillero, IFormFile imagen)
        {
            if (!ModelState.IsValid)
            {
                return View(semillero);
            }

            // Obtener el ID del usuario autenticado
            semillero.UsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Guardar la imagen y manejar errores
            var imagenGuardada = await GuardarImagen(imagen);
            if (imagenGuardada == "Error")
            {
                TempData["Error"] = "El formato de la imagen no es válido. Usa JPG, PNG o JPEG.";
                return View(semillero);
            }
            semillero.Imagen = imagenGuardada;

            // Insertar en la base de datos
            _context.Semilleros.Add(semillero);
            await _context.SaveChangesAsync();

            // Redirigir con mensaje de éxito
            TempData["Success"] = "Semillero creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // 🔹 MÉTODO PRIVADO PARA GUARDAR IMÁGENES
        private async Task<string> GuardarImagen(IFormFile imagen)
        {
            if (imagen == null || imagen.Length == 0)
            {
                return string.Empty;
            }

            string fileExtension = Path.GetExtension(imagen.FileName).ToLower();
            string[] formatosPermitidos = { ".jpg", ".jpeg", ".png" };

            if (!formatosPermitidos.Contains(fileExtension))
            {
                return "Error"; // Retorna error si el formato no es válido
            }

            string fileName = Path.GetFileName(imagen.FileName);
            string directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "images");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagen.CopyToAsync(stream);
            }

            return fileName;
        }


        // 🔹 SOLO DOCENTES Y COORDINADORES PUEDEN EDITAR SEMILLEROS
        [Authorize(Roles = "Docente,Coordinador")]
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
        [Authorize(Roles = "Docente,Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Semillero semillero, IFormFile? imagen, string ImagenActual)
        {
            if (id != semillero.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manejo de imagen
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

                        semillero.Imagen = fileName; // Actualizar con nueva imagen
                    }
                    else
                    {
                        semillero.Imagen = ImagenActual; // Mantener la imagen existente
                    }

                    // Actualizar Semillero en la base de datos
                    _context.Semilleros.Update(semillero);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Semilleros.Any(e => e.Id == semillero.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(semillero);
        }

        // 🔹 SOLO EL COORDINADOR PUEDE ELIMINAR SEMILLEROS
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Buscar el semillero por id
                var semillero = await _context.Semilleros.FindAsync(id);

                if (semillero == null)
                {
                    return NotFound();
                }

                // Eliminar imagen del servidor si existe
                if (!string.IsNullOrEmpty(semillero.Imagen))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", semillero.Imagen);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath); // Eliminar archivo de imagen
                    }
                    else
                    {
                        Console.WriteLine($"No se pudo encontrar la imagen en la ruta: {filePath}");
                    }
                }

                // Eliminar el semillero de la base de datos
                _context.Semilleros.Remove(semillero);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Redirigir a la vista principal después de la eliminación
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al eliminar el semillero: {ex.Message}");
            }
        }

        // 🔹 SOLO ESTUDIANTES, DOCENTES Y COORDINADORES PUEDEN CONSULTAR SEMILLEROS
        [Authorize(Roles = "Estudiante,Docente,Coordinador")]
        public IActionResult Consulta(int id)
        {
            var semillero = _context.Semilleros
                .Include(s => s.Proyectos)
                .FirstOrDefault(s => s.Id == id);

            if (semillero == null)
            {
                return NotFound();
            }

            return View(new List<Semillero> { semillero }); // Devuelve la vista con un solo semillero
        }

        // 🔹 REDIRECCIÓN SI NO TIENE PERMISOS
        public IActionResult AccessDenied()
        {
            TempData["ErrorPermiso"] = "No tienes permisos para acceder a esta acción.";
            return RedirectToAction("Index");
        }

    }
}
