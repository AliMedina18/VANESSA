using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Vanessa.Data;
using Vanessa.Models;

namespace MiAppPdfCrud.Controllers
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
        public IActionResult Configuracion()
        {
            // Obtener todos los proyectos existentes
            var proyectosExistentes = _context.Proyectos.ToList();

            // Pasar los proyectos existentes como modelo
            return View("Configuracion", proyectosExistentes);
        }


        // GET: Proyectos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proyectos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,EquiposInvestigacion,FechaInicio")] Proyecto proyecto,
            IFormFile pdfFile,
            List<int> proyectosSeleccionados) // Recibir los proyectos seleccionados
        {
            if (ModelState.IsValid)
            {
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

                // Agregar el nuevo proyecto
                _context.Add(proyecto);
                await _context.SaveChangesAsync();

                // Lógica para los proyectos seleccionados (puedes editar o asociar los proyectos)
                if (proyectosSeleccionados != null)
                {
                    foreach (var id in proyectosSeleccionados)
                    {
                        var proyectoExistente = await _context.Proyectos.FindAsync(id);
                        if (proyectoExistente != null)
                        {
                            // Aquí puedes agregar lógica para asociar el proyecto seleccionado
                            // con el nuevo proyecto, editarlo o cualquier otra acción que necesites
                        }
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(proyecto);
        }

        // GET: Proyectos/Index
        public async Task<IActionResult> Index()
        {
            return View(await _context.Proyectos.ToListAsync());
        }

        // GET: Proyectos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }

            // Pasamos la ruta del PDF a la vista
            ViewBag.PdfPath = Path.Combine(_uploadPath, proyecto.DocumentoProyecto);

            return View(proyecto);
        }

        // GET: Proyectos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }
            return View(proyecto);
        }

        // POST: Proyectos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,EquiposInvestigacion,FechaInicio,DocumentoProyecto")] Proyecto proyecto, IFormFile pdfFile)
        {
            if (id != proyecto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recuperar el proyecto existente de la base de datos
                    var proyectoExistente = await _context.Proyectos.FirstOrDefaultAsync(p => p.Id == id);
                    if (proyectoExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar los campos enviados por el formulario
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

                return RedirectToAction(nameof(Index));
            }

            return View(proyecto);
        }


        // GET: Proyectos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto == null)
            {
                return NotFound();
            }
            return View(proyecto);
        }

        // POST: Proyectos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proyecto = await _context.Proyectos.FindAsync(id);
            if (proyecto != null)
            {
                var filePath = Path.Combine(_uploadPath, proyecto.DocumentoProyecto);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Proyectos.Remove(proyecto);
                await _context.SaveChangesAsync();
            }
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


    }
}