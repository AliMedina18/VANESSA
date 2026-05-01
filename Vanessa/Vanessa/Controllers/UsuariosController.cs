using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioRepository  _usuarioRepo;
        private readonly IAuthService        _authService;
        private readonly IEmailService       _emailService;
        private readonly ApplicationDbContext _context;   // solo para Roles y batch-delete

        public UsuariosController(
            IUsuarioRepository usuarioRepo,
            IAuthService authService,
            IEmailService emailService,
            ApplicationDbContext context)
        {
            _usuarioRepo  = usuarioRepo;
            _authService  = authService;
            _emailService = emailService;
            _context      = context;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────
        private SelectList GetRolesSelectList(int? selectedRoleId)
        {
            var roles = _context.Roles.Where(r => r.Nombre != "Coordinador" && r.Nombre != "Cliente");
            return new SelectList(roles, "Id", "Nombre", selectedRoleId);
        }

        // ─── Reporte ─────────────────────────────────────────────────────────
        public IActionResult SeleccionarReporte() => View();

        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> GenerarReporte(string tipoReporte)
        {
            var usuarios = (await _usuarioRepo.GetAllActiveAsync()).ToList();
            PdfDocument pdfDocument = tipoReporte switch
            {
                "Tabla"    => CreatePdfTableDocument(usuarios),
                "Gráfica"  => CreatePdfChartDocument(usuarios),
                _          => CreatePdfSimpleDocument(usuarios)
            };

            using var ms = new MemoryStream();
            pdfDocument.Save(ms, false);
            return File(ms.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
        }

        [Obsolete]
        private PdfDocument CreatePdfSimpleDocument(IList<Usuario> usuarios)
        {
            var pdfDocument    = new PdfDocument();
            var fontTitle      = new XFont("Verdana Bold", 16);
            var fontSubTitle   = new XFont("Verdana", 12);
            var fontContent    = new XFont("Verdana", 10);
            var fontPageNumber = new XFont("Verdana Bold", 8);
            var fontFooter     = new XFont("Verdana Italic", 8);
            var fontHeader     = new XFont("Verdana", 10);

            int pageNumber = 1;
            PdfPage  page     = pdfDocument.AddPage();
            XGraphics graphics = XGraphics.FromPdfPage(page);

            XColor lightBlue = XColor.FromArgb(220, 240, 255);
            XColor darkBlue  = XColor.FromArgb(60, 90, 150);

            DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);

            double yPosition  = 100;
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            graphics.DrawLine(new XPen(darkBlue, 1), leftMargin, yPosition - 10, rightMargin, yPosition - 10);
            yPosition += 20;
            graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue),
                new XRect(page.Width - 130, 20, 80, 20), XStringFormats.TopRight);

            foreach (var usuario in usuarios)
            {
                if (yPosition > page.Height - 100)
                {
                    page     = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);
                    graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue),
                        new XRect(page.Width - 130, 20, 80, 20), XStringFormats.TopRight);
                    yPosition = 100;
                }

                graphics.DrawRoundedRectangle(new XPen(lightBlue, 2),
                    leftMargin - 10, yPosition - 15, page.Width - 2 * leftMargin + 10, 85, 10, 10);
                graphics.DrawString($"Nombre: {usuario.Nombre ?? "N/A"}",   fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
                graphics.DrawString($"Documento: {usuario.Documento}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 20);
                graphics.DrawString($"Correo: {usuario.Correo ?? "N/A"}",   fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 40);
                graphics.DrawString($"Rol: {usuario.Rol?.Nombre ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 60);
                yPosition += 100;
            }

            graphics.DrawString($"{pageNumber}", fontPageNumber, new XSolidBrush(darkBlue),
                new XRect(page.Width - 40, 20, 30, 20), XStringFormats.TopRight);
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter,
                new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            return pdfDocument;
        }

        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, XFont fontTitle, XFont fontSubTitle, XColor darkBlue)
        {
            graphics.DrawString("Reporte de Usuarios Activos", fontTitle, new XSolidBrush(darkBlue),
                new XRect(0, 30, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                fontSubTitle, new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, 60, page.Width, 20), XStringFormats.TopCenter);
        }

        [Obsolete]
        private PdfDocument CreatePdfTableDocument(IList<Usuario> usuarios)
        {
            var pdfDocument = new PdfDocument();
            PdfPage   page     = pdfDocument.AddPage();
            XGraphics graphics = XGraphics.FromPdfPage(page);

            var fontTitle   = new XFont("Verdana Bold", 16);
            var fontSubTitle = new XFont("Verdana", 12);
            var fontContent = new XFont("Verdana", 10);
            var fontHeader  = new XFont("Verdana Bold", 10);

            XColor headerBg   = XColor.FromArgb(60, 90, 150);
            XColor headerText = XColor.FromArgb(255, 255, 255);
            XColor rowBg1     = XColor.FromArgb(240, 248, 255);
            XColor rowBg2     = XColor.FromArgb(255, 255, 255);
            XColor border     = XColor.FromArgb(200, 200, 200);
            XColor textColor  = XColor.FromArgb(60, 90, 150);

            double tableWidth  = 550;
            double leftMargin  = (page.Width - tableWidth) / 2;
            double contentH    = usuarios.Count * 25 + 60;
            double yPosition   = Math.Max((page.Height - contentH) / 2 - 110, 100);

            DrawTitle(graphics, page, "Reporte de Usuarios Activos",
                "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                fontTitle, fontSubTitle, headerBg, yPosition - 60);

            DrawTableHeader(graphics, ref yPosition, leftMargin, tableWidth, fontHeader, headerBg, headerText);

            int rowIndex = 0;
            foreach (var usuario in usuarios)
            {
                if (yPosition > page.Height - 100)
                {
                    page     = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    yPosition = 100;
                    DrawTitle(graphics, page, "Reporte de Usuarios Activos",
                        "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                        fontTitle, fontSubTitle, headerBg, yPosition - 60);
                    DrawTableHeader(graphics, ref yPosition, leftMargin, tableWidth, fontHeader, headerBg, headerText);
                }
                DrawTableRow(graphics, usuario, ref yPosition, leftMargin, fontContent,
                    rowIndex % 2 == 0 ? rowBg1 : rowBg2, textColor, border, tableWidth);
                rowIndex++;
            }

            return pdfDocument;
        }

        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, string title, string subTitle,
            XFont titleFont, XFont subTitleFont, XColor titleColor, double yPosition)
        {
            graphics.DrawString(title, titleFont, new XSolidBrush(titleColor),
                new XRect(0, yPosition, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString(subTitle, subTitleFont, new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, yPosition + 30, page.Width, 20), XStringFormats.TopCenter);
        }

        private void DrawTableHeader(XGraphics g, ref double y, double left, double tableW,
            XFont font, XColor bg, XColor fg)
        {
            const double c1 = 150, c2 = 100, c3 = 200, c4 = 100, h = 30;
            g.DrawRectangle(new XSolidBrush(bg), left, y - 5, tableW, h);
            g.DrawString("Nombre",    font, new XSolidBrush(fg), new XRect(left,           y, c1, h), XStringFormats.Center);
            g.DrawString("Documento", font, new XSolidBrush(fg), new XRect(left + c1,      y, c2, h), XStringFormats.Center);
            g.DrawString("Correo",    font, new XSolidBrush(fg), new XRect(left + c1 + c2, y, c3, h), XStringFormats.Center);
            g.DrawString("Rol",       font, new XSolidBrush(fg), new XRect(left + c1 + c2 + c3, y, c4, h), XStringFormats.Center);
            y += h + 10;
        }

        private void DrawTableRow(XGraphics g, Usuario u, ref double y, double left,
            XFont font, XColor bg, XColor fg, XColor border, double tableW)
        {
            const double c1 = 150, c2 = 100, c3 = 200, c4 = 100, rh = 25;
            g.DrawRectangle(new XSolidBrush(bg), left, y, tableW, rh);
            g.DrawString(u.Nombre ?? "N/A",              font, new XSolidBrush(fg), new XRect(left,           y, c1, rh), XStringFormats.Center);
            g.DrawString(u.Documento.ToString(), font, new XSolidBrush(fg), new XRect(left + c1,   y, c2, rh), XStringFormats.Center);
            g.DrawString(u.Correo ?? "N/A",              font, new XSolidBrush(fg), new XRect(left + c1 + c2, y, c3, rh), XStringFormats.Center);
            g.DrawString(u.Rol?.Nombre ?? "N/A",         font, new XSolidBrush(fg), new XRect(left + c1 + c2 + c3, y, c4, rh), XStringFormats.Center);
            y += rh;
        }

        [Obsolete]
        private PdfDocument CreatePdfChartDocument(IList<Usuario> usuarios)
        {
            var pdfDocument = new PdfDocument();
            var page     = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            DrawTitle(graphics, page, "Reporte de Usuarios Activos",
                "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                new XFont("Verdana Bold", 16), new XFont("Verdana", 12),
                XColor.FromArgb(60, 90, 150));

            DrawBarChart(graphics, usuarios, page);
            return pdfDocument;
        }

        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, string mainTitle, string subTitle,
            XFont fontTitle, XFont fontSubTitle, XColor backgroundColor)
        {
            double headerHeight = 80;
            graphics.DrawRectangle(new XSolidBrush(backgroundColor), 0, 0, page.Width, headerHeight);
            graphics.DrawLine(new XPen(XColor.FromArgb(100, 255, 255, 255), 2), 0, headerHeight, page.Width, headerHeight);
            graphics.DrawString(mainTitle,  fontTitle,    XBrushes.White,
                new XRect(0, 10, page.Width, headerHeight / 2), XStringFormats.Center);
            graphics.DrawString(subTitle,   fontSubTitle, new XSolidBrush(XColor.FromArgb(220, 220, 220)),
                new XRect(0, headerHeight / 2, page.Width, headerHeight / 2), XStringFormats.Center);
        }

        [Obsolete]
        private void DrawBarChart(XGraphics graphics, IList<Usuario> usuarios, PdfPage page)
        {
            int barWidth = 70;
            double maxBarHeight = 200;
            double chartHeight = 300;
            double spacing = 20;
            var usuariosPorRol = usuarios
                .GroupBy(u => u.Rol?.Nombre ?? "Sin Rol")
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToList();

            int maxCantidad  = usuariosPorRol.Max(r => r.Cantidad);
            int numberOfBars = usuariosPorRol.Count;
            double totalW    = barWidth * numberOfBars + spacing * (numberOfBars - 1);
            double chartX    = (page.Width - totalW) / 2;
            double chartY    = 120;

            XColor gridColor = XColor.FromArgb(200, 200, 200);
            double gridStep  = maxBarHeight / 5.0;
            for (int i = 0; i <= 5; i++)
            {
                double y = chartY + chartHeight - gridStep * i;
                graphics.DrawLine(new XPen(gridColor, 1), chartX - 20, y, chartX + totalW, y);
                graphics.DrawString(((maxCantidad / 5) * i).ToString(), new XFont("Verdana", 8),
                    XBrushes.Black, new XRect(chartX - 50, y - 10, 30, 20), XStringFormats.CenterRight);
            }

            int curX   = (int)chartX;
            var colors = new[] { XBrushes.SkyBlue, XBrushes.LightGreen, XBrushes.Orange, XBrushes.Purple };
            var lFont  = new XFont("Verdana", 10);
            int ci     = 0;
            foreach (var d in usuariosPorRol)
            {
                double barH = (d.Cantidad / (double)maxCantidad) * maxBarHeight;
                graphics.DrawRectangle(colors[ci % colors.Length], curX, chartY + (chartHeight - barH), barWidth, barH);
                graphics.DrawString(d.Cantidad.ToString(), lFont, XBrushes.Black,
                    new XRect(curX, chartY + (chartHeight - barH) - 20, barWidth, 20), XStringFormats.TopCenter);
                graphics.DrawString(d.Rol, lFont, XBrushes.Black,
                    new XRect(curX, chartY + chartHeight + 10, barWidth, 20), XStringFormats.TopCenter);
                curX += (int)(barWidth + spacing);
                ci++;
            }
        }

        // ─── Reporte Inactivos ────────────────────────────────────────────────
        [Obsolete]
        public async Task<IActionResult> DescargarReporteInactivos()
        {
            var usuariosInactivos = (await _usuarioRepo.GetAllInactiveAsync()).ToList();

            var pdfDocument    = new PdfDocument();
            var fontTitle      = new XFont("Verdana Bold", 16);
            var fontSubTitle   = new XFont("Verdana", 12);
            var fontContent    = new XFont("Verdana", 10);
            var fontPageNumber = new XFont("Verdana Bold", 8);
            var fontFooter     = new XFont("Verdana Italic", 8);

            var page     = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(page);
            var darkBlue = XColor.FromArgb(60, 90, 150);

            graphics.DrawString("Reporte de Usuarios Inactivos", fontTitle, new XSolidBrush(darkBlue),
                new XRect(0, 30, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                fontSubTitle, new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, 60, page.Width, 20), XStringFormats.TopCenter);

            double y = 100, left = 50, right = page.Width - 50;
            graphics.DrawLine(new XPen(darkBlue, 1), left, y - 10, right, y - 10);
            void DrawHeaderRow(XGraphics g, double yPos)
            {
                g.DrawString("Nombre",         fontContent, new XSolidBrush(darkBlue), left,       yPos);
                g.DrawString("Documento",      fontContent, new XSolidBrush(darkBlue), left + 150, yPos);
                g.DrawString("Correo",         fontContent, new XSolidBrush(darkBlue), left + 300, yPos);
                g.DrawString("Fecha Inactivo", fontContent, new XSolidBrush(darkBlue), left + 450, yPos);
            }
            DrawHeaderRow(graphics, y);
            y += 20;

            foreach (var u in usuariosInactivos)
            {
                if (y > page.Height - 100)
                {
                    page     = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    graphics.DrawLine(new XPen(darkBlue, 1), left, 90, right, 90);
                    DrawHeaderRow(graphics, 100);
                    y = 120;
                }
                graphics.DrawString(u.Nombre ?? "N/A",                fontContent, new XSolidBrush(darkBlue), left,       y);
                graphics.DrawString(u.Documento.ToString(), fontContent, new XSolidBrush(darkBlue), left + 150, y);
                graphics.DrawString(u.Correo ?? "N/A",                fontContent, new XSolidBrush(darkBlue), left + 300, y);
                graphics.DrawString(u.FechaInactivo?.ToString("dd/MM/yyyy") ?? "N/A", fontContent, new XSolidBrush(darkBlue), left + 450, y);
                y += 20;
            }

            graphics.DrawString("Página 1", fontPageNumber, new XSolidBrush(darkBlue),
                new XRect(page.Width - 40, page.Height - 30, 30, 20), XStringFormats.TopRight);
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter,
                new XSolidBrush(XColor.FromArgb(169, 169, 169)),
                new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            using var ms = new MemoryStream();
            pdfDocument.Save(ms, false);
            ms.Position = 0;
            return File(ms.ToArray(), "application/pdf", "Reporte_Usuarios_Inactivos.pdf");
        }

        // ─── Index con paginación ─────────────────────────────────────────────
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            const int pageSize = 3;
            var todos = (await _usuarioRepo.GetAllActiveAsync()).ToList();

            if (!string.IsNullOrEmpty(search))
                todos = todos.Where(u => u.Nombre != null &&
                    u.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            int total      = todos.Count;
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);
            var paginated  = todos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Search = search;
            return View(new UsuariosIndexViewModel
            {
                Usuarios   = paginated,
                PageNumber = page,
                TotalPages = totalPages,
                PageSize   = pageSize
            });
        }

        // ─── Inactivos ────────────────────────────────────────────────────────
        public async Task<IActionResult> Inactivos(string search = "")
        {
            var inactivos = (await _usuarioRepo.GetAllInactiveAsync()).ToList();

            if (!string.IsNullOrEmpty(search))
                inactivos = inactivos.Where(u => u.Nombre != null &&
                    u.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.Search = search;
            return View(inactivos);
        }

        // ─── Create / Edit ────────────────────────────────────────────────────
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            var usuario = id.HasValue
                ? await _usuarioRepo.GetByIdAsync(id.Value) ?? new Usuario()
                : new Usuario();

            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(
            int? id,
            [Bind("Id,Nombre,Documento,Correo,Contraseña,ConfirmarContraseña,RolId,Activo")] Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                return View(usuario);
            }

            if (id == null || id == 0)   // ── Crear ──
            {
                if (string.IsNullOrEmpty(usuario.Contraseña))
                {
                    usuario.Contraseña        = "Contraseña123!";
                    usuario.ConfirmarContraseña = usuario.Contraseña;
                }

                if (!_authService.EsContraseñaSegura(usuario.Contraseña))
                {
                    ModelState.AddModelError("", "La contraseña no cumple con los requisitos de seguridad.");
                    ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                    return View(usuario);
                }

                usuario.Contraseña  = _authService.ConvertirContraseña(usuario.Contraseña);
                usuario.Activo      = true;
                usuario.FechaInactivo = null;

                await _usuarioRepo.AddAsync(usuario);
                await _usuarioRepo.AsignarPermisosClienteAsync(usuario.Id);

                await _emailService.SendEmailAsync(
                    usuario.Correo ?? "",
                    "Bienvenido a nuestra plataforma",
                    $"<p>Hola {usuario.Nombre},</p><p>Tu cuenta ha sido creada exitosamente. " +
                    $"Tu contraseña inicial es: <b>Contraseña123!</b></p>");
            }
            else                          // ── Editar ──
            {
                var existente = await _usuarioRepo.GetByIdAsync(id.Value);
                if (existente == null) return NotFound();

                existente.Nombre    = usuario.Nombre;
                existente.Documento = usuario.Documento;
                existente.Correo    = usuario.Correo;
                existente.RolId     = usuario.RolId;

                if (!string.IsNullOrEmpty(usuario.Contraseña))
                {
                    if (!_authService.EsContraseñaSegura(usuario.Contraseña))
                    {
                        ModelState.AddModelError("", "La contraseña no cumple con los requisitos de seguridad.");
                        ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                        return View(usuario);
                    }
                    existente.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                }

                await _usuarioRepo.UpdateAsync(existente);

                await _emailService.SendEmailAsync(
                    existente.Correo ?? "",
                    "Actualización de cuenta",
                    $"<p>Hola {existente.Nombre},</p><p>Tu cuenta ha sido actualizada exitosamente.</p>");
            }

            TempData["Success"] = id == null || id == 0 ? "Usuario creado exitosamente." : "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ─── Eliminar (marcar inactivo) ───────────────────────────────────────
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo      = false;
            usuario.FechaInactivo = DateTime.UtcNow;
            await _usuarioRepo.UpdateAsync(usuario);

            TempData["Success"] = "Usuario marcado como inactivo.";
            return RedirectToAction(nameof(Index));
        }

        // ─── Reactivar ────────────────────────────────────────────────────────
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.Activo       = true;
            usuario.FechaInactivo = null;
            await _usuarioRepo.UpdateAsync(usuario);

            TempData["Success"] = "Usuario reactivado correctamente.";
            return RedirectToAction(nameof(Inactivos));
        }

        // ─── Eliminar definitivamente ─────────────────────────────────────────
        public async Task<IActionResult> EliminarDefinitivamente(int id)
        {
            var usuario = await _usuarioRepo.GetByIdAsync(id);
            if (usuario == null) return NotFound();

            var correo = usuario.Correo;
            var nombre = usuario.Nombre;
            await _usuarioRepo.DeleteAsync(usuario);

            await _emailService.SendEmailAsync(
                correo ?? "",
                "Cuenta eliminada",
                $"<p>Hola {nombre},</p><p>Tu cuenta ha sido eliminada definitivamente del sistema. " +
                $"Si tienes alguna pregunta, por favor contáctanos.</p>");

            TempData["Success"] = "Usuario eliminado definitivamente.";
            return RedirectToAction(nameof(Inactivos));
        }

        // ─── Eliminación automática de inactivos (usada por BackgroundService) ─
        public async Task EliminarUsuariosInactivosAutomaticamente()
        {
            var fechaLimite = DateTime.UtcNow.AddDays(-30);
            var viejos = await _context.Usuarios
                .Where(u => !u.Activo && u.FechaInactivo <= fechaLimite)
                .ToListAsync();

            if (viejos.Any())
            {
                _context.Usuarios.RemoveRange(viejos);
                await _context.SaveChangesAsync();
            }
        }
    }
}
