using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO.Compression;
using System.Security.Claims;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    public class ProyectosController : Controller
    {
        private readonly IProyectoRepository  _proyectoRepo;
        private readonly ISemilleroRepository _semilleroRepo;
        private readonly IWebHostEnvironment  _env;

        private string UploadPath => Path.Combine(_env.WebRootPath, "uploads");

        public ProyectosController(
            IProyectoRepository proyectoRepo,
            ISemilleroRepository semilleroRepo,
            IWebHostEnvironment env)
        {
            _proyectoRepo  = proyectoRepo;
            _semilleroRepo = semilleroRepo;
            _env           = env;
            if (!Directory.Exists(Path.Combine(env.WebRootPath, "uploads")))
                Directory.CreateDirectory(Path.Combine(env.WebRootPath, "uploads"));
        }

        // GET: Proyectos/Index
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Index()
        {
            return View(await _proyectoRepo.GetAllAsync());
        }

        // GET: Proyectos/Configuracion
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public async Task<IActionResult> Configuracion(string? search)
        {
            var userId   = ObtenerUsuarioId();
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var todos    = await _proyectoRepo.GetAllAsync();
            IEnumerable<Proyecto> proyectos = todos;

            if (userRole is "Docente" or "Estudiante")
                proyectos = proyectos.Where(p => p.UsuarioCoordenadorId == userId);

            if (!string.IsNullOrEmpty(search))
            {
                proyectos = proyectos.Where(p =>
                    p.Nombre != null && p.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase));
                ViewData["SearchQuery"] = search;
            }
            return View(proyectos.ToList());
        }

        // GET: Proyectos/Create
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Semilleros = new SelectList(await _semilleroRepo.GetAllAsync(), "Id", "Nombre");
            return View();
        }

        // POST: Proyectos/Create
        [HttpPost]
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,FechaInicio,SemilleroId")] Proyecto proyecto,
            IFormFile? pdfFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Semilleros = new SelectList(await _semilleroRepo.GetAllAsync(), "Id", "Nombre");
                return View(proyecto);
            }
            proyecto.UsuarioCoordenadorId = ObtenerUsuarioId();
            if (pdfFile != null && pdfFile.Length > 0)
                proyecto.DocumentoProyecto = await GuardarArchivoAsync(pdfFile, UploadPath);

            await _proyectoRepo.AddAsync(proyecto);
            TempData["Success"] = "Proyecto creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Proyectos/Details/5
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Details(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null) return NotFound();
            return View(proyecto);
        }

        // GET: Proyectos/Edit/5
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public async Task<IActionResult> Edit(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null) return NotFound();
            if (User.IsInRole("Estudiante") && proyecto.UsuarioCoordenadorId != ObtenerUsuarioId())
            {
                TempData["Error"] = "No puedes editar proyectos que no te pertenecen.";
                return RedirectToAction(nameof(Index));
            }
            return View(proyecto);
        }

        // POST: Proyectos/Edit/5
        [HttpPost]
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Proyecto proyecto, IFormFile? pdfFile)
        {
            if (id != proyecto.Id) return NotFound();
            if (!ModelState.IsValid) return View(proyecto);

            var existente = await _proyectoRepo.GetByIdAsync(id);
            if (existente == null) return NotFound();

            if (User.IsInRole("Estudiante") && existente.UsuarioCoordenadorId != ObtenerUsuarioId())
            {
                TempData["Error"] = "No puedes editar proyectos que no te pertenecen.";
                return RedirectToAction(nameof(Index));
            }

            existente.Nombre = proyecto.Nombre;
            existente.FechaInicio = proyecto.FechaInicio;
            existente.FechaActualizacion = DateTime.UtcNow;

            if (pdfFile != null && pdfFile.Length > 0)
            {
                EliminarArchivoSiExiste(Path.Combine(UploadPath, existente.DocumentoProyecto ?? ""));
                existente.DocumentoProyecto = await GuardarArchivoAsync(pdfFile, UploadPath);
            }
            await _proyectoRepo.UpdateAsync(existente);
            TempData["Success"] = "Proyecto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Proyectos/Delete/5
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null) return NotFound();
            return View(proyecto);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto != null)
            {
                EliminarArchivoSiExiste(Path.Combine(UploadPath, proyecto.DocumentoProyecto ?? ""));
                await _proyectoRepo.DeleteAsync(proyecto);
            }
            TempData["Success"] = "Proyecto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Proyectos/Download/5
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Download(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null || string.IsNullOrEmpty(proyecto.DocumentoProyecto)) return NotFound();
            var path = Path.Combine(UploadPath, proyecto.DocumentoProyecto);
            if (!System.IO.File.Exists(path)) return NotFound();
            return File(await System.IO.File.ReadAllBytesAsync(path), "application/pdf", proyecto.DocumentoProyecto);
        }

        // GET: Proyectos/Consulta/5
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Consulta(int id)
        {
            var proyecto = await _proyectoRepo.GetByIdAsync(id);
            if (proyecto == null) return NotFound();

            var allowedExt = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
            var projectDir = Path.Combine(UploadPath, id.ToString());
            var files = Directory.Exists(projectDir)
                ? Directory.GetFiles(projectDir)
                    .Where(f => allowedExt.Contains(Path.GetExtension(f).ToLower()))
                    .Select(Path.GetFileName)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .Select(f => f!)
                    .ToList()
                : new List<string>();

            ViewBag.Files             = files;
            ViewBag.ProjectId         = id;
            ViewBag.DocumentoProyecto = !string.IsNullOrEmpty(proyecto.DocumentoProyecto) && files.Contains(proyecto.DocumentoProyecto)
                ? proyecto.DocumentoProyecto : null;
            return View(proyecto);
        }

        // POST: Proyectos/UploadFile
        [HttpPost]
        [Authorize(Roles = "Coordinador,Docente,Estudiante")]
        public async Task<IActionResult> UploadFile(int id, IFormFile? file)
        {
            if (file != null && file.Length > 0)
            {
                var projectDir = Path.Combine(UploadPath, id.ToString());
                if (!Directory.Exists(projectDir)) Directory.CreateDirectory(projectDir);
                await GuardarArchivoAsync(file, projectDir);
            }
            return RedirectToAction("Consulta", new { id });
        }

        // GET: Proyectos/DownloadFile
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public IActionResult DownloadFile(int id, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest("Nombre de archivo inválido.");
            var path = Path.Combine(UploadPath, id.ToString(), fileName);
            if (!System.IO.File.Exists(path)) return NotFound();
            return File(System.IO.File.ReadAllBytes(path), "application/octet-stream", fileName);
        }

        // GET: Proyectos/DownloadAllFiles
        [Authorize(Roles = "Cliente,Estudiante,Docente,Coordinador")]
        public IActionResult DownloadAllFiles(int id)
        {
            var projectDir = Path.Combine(UploadPath, id.ToString());
            if (!Directory.Exists(projectDir)) return NotFound("No hay archivos en este proyecto.");
            var files = Directory.GetFiles(projectDir);
            if (files.Length == 0) return NotFound("No hay archivos en este proyecto.");

            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                foreach (var f in files) zip.CreateEntryFromFile(f, Path.GetFileName(f));

            return File(ms.ToArray(), "application/zip", $"Proyecto_{id}_Archivos.zip");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private int ObtenerUsuarioId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private static async Task<string> GuardarArchivoAsync(IFormFile file, string dir)
        {
            var name = Path.GetFileName(file.FileName);
            using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
            await file.CopyToAsync(s);
            return name;
        }

        private static void EliminarArchivoSiExiste(string path)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }
    }
}
