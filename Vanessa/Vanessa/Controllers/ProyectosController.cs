using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.IO.Compression;
using System.Security.Claims;
using System.Threading.Tasks;
using Vanessa.Data;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public ProyectosController(ApplicationDbContext context)
        {
            _context = context;
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        // GET: Proyectos/Configuracion
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public IActionResult Configuracion(string search)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var proyectos = _context.Proyectos.AsQueryable();

            if (userRole == "Docente" || userRole == "Estudiante")
            {
                proyectos = proyectos.Where(p => p.UsuarioId == userId);
            }

            if (!string.IsNullOrEmpty(search))
            {
                proyectos = proyectos.Where(p => p.Nombre != null && p.Nombre.Contains(search));
                ViewData["SearchQuery"] = search;
            }

            return View("Configuracion", proyectos.ToList());
        }

        // 🔹 SOLO COORDINADOR, DOCENTE Y ESTUDIANTE PUEDEN CREAR PROYECTOS
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public IActionResult Create()
        {
            ViewBag.Semilleros = new SelectList(_context.Semilleros, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,EquiposInvestigacion,FechaInicio,SemilleroId")] Proyecto proyecto,
            IFormFile pdfFile,
            List<int> proyectosSeleccionados)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Semilleros = new SelectList(_context.Semilleros, "Id", "Nombre");
                return View(proyecto);
            }

            // Asignar el ID del usuario autenticado al proyecto
            proyecto.UsuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            // Subir el archivo PDF
            if (pdfFile != null && pdfFile.Length > 0)
            {
                var fileName = Path.GetFileName(pdfFile.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await pdfFile.CopyToAsync(stream);
                }
                proyecto.DocumentoProyecto = fileName;
            }

            _context.Add(proyecto);
            await _context.SaveChangesAsync();

            // Asignar los proyectos seleccionados si existen
            if (proyectosSeleccionados != null)
            {
                foreach (var id in proyectosSeleccionados)
                {
                    var proyectoExistente = await _context.Proyectos.FindAsync(id);
                    if (proyectoExistente != null)
                    {
                        // Lógica para asociar proyectos aquí
                    }
                }
            }

            TempData["Success"] = "Proyecto creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // 🔹 CLIENTES, ESTUDIANTES, DOCENTES Y COORDINADORES PUEDEN VER PROYECTOS
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Index()
        {
            var proyectos = await _context.Proyectos
                .OrderByDescending(p => p.FechaInicio) // Ordenar por fecha de inicio más reciente
                .ToListAsync();

            return View(proyectos);
        }

        // 🔹 CLIENTES, ESTUDIANTES, DOCENTES Y COORDINADORES PUEDEN VER DETALLES
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Details(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }

            // Pasamos la ruta del PDF a la vista
            ViewBag.PdfPath = Path.Combine(_uploadPath, proyecto.DocumentoProyecto ?? "");

            return View(proyecto);
        }

        // 🔹 SOLO COORDINADOR, DOCENTE Y ESTUDIANTE PUEDEN EDITAR PROYECTOS
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public async Task<IActionResult> Edit(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }

            // Verificar si el usuario es Estudiante y el proyecto no es suyo
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (User.IsInRole("Estudiante") && userIdClaim != null && int.TryParse(userIdClaim, out int userId))
            {
                if (proyecto.UsuarioId != userId)
                {
                    TempData["Error"] = "No puedes editar proyectos que no te pertenecen.";
                    return RedirectToAction(nameof(Index));

                }
            }


            return View(proyecto);
        }

        [HttpPost]
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Proyecto proyecto, IFormFile pdfFile)
        {
            if (id != proyecto.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(proyecto);
            }

            try
            {
                // Recuperar el proyecto existente de la base de datos
                var proyectoExistente = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == id);
                if (proyectoExistente == null)
                {
                    return NotFound();
                }

                // Obtener el ID del usuario autenticado
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (User.IsInRole("Estudiante") && userIdClaim != null && int.TryParse(userIdClaim, out int userId))
                {
                    if (proyectoExistente.UsuarioId != userId)
                    {
                        TempData["Error"] = "No puedes editar proyectos que no te pertenecen.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Verificar si el usuario es Estudiante e intenta cambiar el semillero
                if (User.IsInRole("Estudiante") && proyectoExistente.SemilleroId != proyecto.SemilleroId)
                {
                    TempData["Error"] = "No puedes cambiar el semillero sin aprobación del Coordinador.";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar los campos editables
                proyectoExistente.Nombre = proyecto.Nombre;
                proyectoExistente.EquiposInvestigacion = proyecto.EquiposInvestigacion;
                proyectoExistente.FechaInicio = proyecto.FechaInicio;

                // Si se sube un nuevo archivo, reemplazar el archivo existente
                if (pdfFile != null && pdfFile.Length > 0)
                {
                    // Eliminar el archivo existente si corresponde
                    if (!string.IsNullOrEmpty(proyectoExistente.DocumentoProyecto))
                    {
                        var existingFilePath = Path.Combine(_uploadPath, proyectoExistente.DocumentoProyecto);
                        if (System.IO.File.Exists(existingFilePath))
                        {
                            System.IO.File.Delete(existingFilePath);
                        }
                    }

                    // Guardar el nuevo archivo
                    var fileName = Path.GetFileName(pdfFile.FileName);
                    var filePath = Path.Combine(_uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await pdfFile.CopyToAsync(stream);
                    }
                    proyectoExistente.DocumentoProyecto = fileName;
                }

                // Guardar cambios en la base de datos
                _context.Update(proyectoExistente);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Proyecto actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Proyectos.Any(e => e.Id == proyecto.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // 🔹 SOLO EL COORDINADOR PUEDE ELIMINAR PROYECTOS
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }
            return View(proyecto);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                var filePath = Path.Combine(_uploadPath, proyecto.DocumentoProyecto ?? "");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Proyecto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }


        // GET: Proyectos/Download/5

        public async Task<IActionResult> Download(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null || string.IsNullOrEmpty(proyecto.DocumentoProyecto))
            {
                return NotFound(); // Si no hay proyecto o no tiene documento asociado
            }

            var filePath = Path.Combine(_uploadPath, proyecto.DocumentoProyecto);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(); // Si el archivo no existe
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, "application/pdf", proyecto.DocumentoProyecto);
        }

        // GET: Proyectos/Consulta
        public async Task<IActionResult> Consulta(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }

            // Obtener la lista de archivos del proyecto (Word, PDF, Excel, etc.) asociados a este proyecto
            var projectDirectory = Path.Combine(_uploadPath, id.ToString());
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };

            var files = Directory.Exists(projectDirectory)
                ? Directory.GetFiles(projectDirectory)
                    .Where(f => allowedExtensions.Contains(Path.GetExtension(f)?.ToLower() ?? string.Empty))
                    .Select(f => Path.GetFileName(f) ?? string.Empty)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToList()
                : new List<string>();

            ViewBag.Files = files;
            ViewBag.ProjectId = id;
            ViewBag.DocumentoProyecto = (!string.IsNullOrEmpty(proyecto.DocumentoProyecto) && files.Contains(proyecto.DocumentoProyecto))
                ? proyecto.DocumentoProyecto
                : null;

            return View(proyecto);
        }

        // POST: Subir archivo al proyecto
        [HttpPost]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var projectDirectory = Path.Combine(_uploadPath, id.ToString());
                if (!Directory.Exists(projectDirectory))
                {
                    Directory.CreateDirectory(projectDirectory);
                }

                var fileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(projectDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            return RedirectToAction("Consulta", new { id });
        }

        // GET: Descargar archivo
        public IActionResult DownloadFile(int id, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("El nombre del archivo no puede estar vacío.");
            }

            var projectDirectory = Path.Combine(_uploadPath, id.ToString());
            var filePath = Path.Combine(projectDirectory, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("El archivo solicitado no existe.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // GET: Descargar todos los archivos de un proyecto en un ZIP
        public IActionResult DownloadAllFiles(int id)
        {
            var projectDirectory = Path.Combine(_uploadPath, id.ToString());
            if (!Directory.Exists(projectDirectory))
            {
                return NotFound("No hay archivos para descargar en este proyecto.");
            }

            var files = Directory.GetFiles(projectDirectory);
            if (files.Length == 0)
            {
                return NotFound("No hay archivos en este proyecto.");
            }

            var zipFileName = $"Proyecto_{id}_Archivos.zip";
            var zipFilePath = Path.Combine(_uploadPath, zipFileName);

            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFilePath, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }

            var zipBytes = System.IO.File.ReadAllBytes(zipFilePath);
            System.IO.File.Delete(zipFilePath); // Eliminar el ZIP después de enviarlo

            return File(zipBytes, "application/zip", zipFileName);
        }

    }
}