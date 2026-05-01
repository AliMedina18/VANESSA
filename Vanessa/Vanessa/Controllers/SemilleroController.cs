using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Security.Claims;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    [Authorize]
    public class SemilleroController : Controller
    {
        private readonly ISemilleroRepository _semilleroRepo;
        private readonly IWebHostEnvironment  _env;

        public SemilleroController(ISemilleroRepository semilleroRepo, IWebHostEnvironment env)
        {
            _semilleroRepo = semilleroRepo;
            _env           = env;
        }

        [Authorize(Roles = "Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Index()
        {
            if (TempData["ErrorPermiso"] != null)
                ViewBag.ErrorPermiso = TempData["ErrorPermiso"];
            return View(await _semilleroRepo.GetAllAsync());
        }

        [Obsolete]
        [Authorize(Roles = "Docente,Coordinador")]
        public async Task<IActionResult> GenerarPdf()
        {
            var semilleros = await _semilleroRepo.GetAllAsync();
            var pdfDocument = new PdfDocument();
            var fontTitle   = new XFont("Verdana Bold", 16);
            var fontSubTitle = new XFont("Verdana", 12);
            var fontContent = new XFont("Verdana", 10);
            var fontPageNum = new XFont("Verdana Bold", 8);
            var fontFooter  = new XFont("Verdana Italic", 8);
            var fontHeader  = new XFont("Verdana", 10);
            var darkBlue    = XColor.FromArgb(60, 90, 150);
            var lightBlue   = XColor.FromArgb(220, 240, 255);

            int pageNumber = 1;
            var page    = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(page);
            DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);

            double y = 100, left = 50, right = page.Width - 50;
            graphics.DrawLine(new XPen(darkBlue, 1), left, y - 10, right, y - 10);
            y += 20;
            graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue),
                new XRect(page.Width - 190, 20, 150, 20), XStringFormats.TopRight);

            foreach (var s in semilleros)
            {
                if (y > page.Height - 100)
                {
                    page = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);
                    graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue),
                        new XRect(page.Width - 150, 20, 150, 20), XStringFormats.TopRight);
                    y = 100;
                }
                graphics.DrawRoundedRectangle(new XPen(lightBlue, 2), left - 10, y - 15, page.Width - 2 * left + 10, 85, 10, 10);
                graphics.DrawString($"Nombre: {s.Nombre ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), left, y);
                graphics.DrawString($"Descripción: {s.Descripcion ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), left, y + 20);
                y += 100;
            }

            graphics.DrawString($"{pageNumber}", fontPageNum, new XSolidBrush(darkBlue),
                new XRect(page.Width - 40, 20, 30, 20), XStringFormats.TopRight);
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter,
                new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            var stream = new MemoryStream();
            pdfDocument.Save(stream, false);
            stream.Position = 0;
            return File(stream, "application/pdf", "Semilleros.pdf");
        }

        private void DrawTitle(XGraphics g, PdfPage p, XFont ft, XFont fs, XColor c)
        {
            g.DrawString("Listado de Semilleros", ft, new XSolidBrush(c),
                new XRect(0, 30, p.Width.Point, 40), XStringFormats.TopCenter);
            g.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                fs, new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, 60, p.Width.Point, 20), XStringFormats.TopCenter);
        }

        [Authorize(Roles = "Docente,Coordinador")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Docente,Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Semillero semillero, IFormFile? imagen)
        {
            if (!ModelState.IsValid) return View(semillero);

            semillero.UsuarioCoordinadorId = ObtenerUsuarioId();
            var img = await GuardarImagenAsync(imagen);
            if (img == "Error")
            {
                TempData["Error"] = "El formato de la imagen no es válido. Usa JPG, PNG o JPEG.";
                return View(semillero);
            }
            semillero.Imagen = img;
            await _semilleroRepo.AddAsync(semillero);
            TempData["Success"] = "Semillero creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Docente,Coordinador")]
        public async Task<IActionResult> Edit(int id)
        {
            var semillero = await _semilleroRepo.GetByIdAsync(id);
            if (semillero == null) return NotFound();
            return View(semillero);
        }

        [HttpPost]
        [Authorize(Roles = "Docente,Coordinador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Semillero semillero, IFormFile? imagen, string? ImagenActual)
        {
            if (id != semillero.Id) return NotFound();
            if (!ModelState.IsValid) return View(semillero);

            if (imagen != null && imagen.Length > 0)
            {
                var fn = Path.GetFileName(imagen.FileName);
                var dir = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                using var s = new FileStream(Path.Combine(dir, fn), FileMode.Create);
                await imagen.CopyToAsync(s);
                semillero.Imagen = fn;
            }
            else
            {
                semillero.Imagen = ImagenActual;
            }
            await _semilleroRepo.UpdateAsync(semillero);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Delete(int id)
        {
            var semillero = await _semilleroRepo.GetByIdAsync(id);
            if (semillero == null) return NotFound();

            if (!string.IsNullOrEmpty(semillero.Imagen))
            {
                var fp = Path.Combine(_env.WebRootPath, "images", semillero.Imagen);
                if (System.IO.File.Exists(fp)) System.IO.File.Delete(fp);
            }
            await _semilleroRepo.DeleteAsync(semillero);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Estudiante,Docente,Coordinador")]
        public async Task<IActionResult> Consulta(int id)
        {
            var semillero = await _semilleroRepo.GetByIdWithProyectosAsync(id);
            if (semillero == null) return NotFound();
            return View(new List<Semillero> { semillero });
        }

        public IActionResult AccessDenied()
        {
            TempData["ErrorPermiso"] = "No tienes permisos para acceder a esta acción.";
            return RedirectToAction("Index");
        }

        private int ObtenerUsuarioId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        private async Task<string> GuardarImagenAsync(IFormFile? imagen)
        {
            if (imagen == null || imagen.Length == 0) return string.Empty;
            var ext = Path.GetExtension(imagen.FileName).ToLower();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(ext)) return "Error";

            var dir = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var fn = Path.GetFileName(imagen.FileName);
            using var s = new FileStream(Path.Combine(dir, fn), FileMode.Create);
            await imagen.CopyToAsync(s);
            return fn;
        }
    }
}
