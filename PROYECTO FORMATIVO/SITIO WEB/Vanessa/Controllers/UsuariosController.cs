using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Vanessa.Data;
using Vanessa.Models;
using Vanessa.Services;

namespace Vanessa.Controllers
{
    [Authorize(Roles = "Coordinador")] // Solo accesible por Coordinadores
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly EmailService _emailService;

        // Constructor
        public UsuariosController(ApplicationDbContext context, AuthService authService, EmailService emailService)
        {
            _context = context;
            _authService = authService;
            _emailService = emailService;
        }

        // Cargar roles, excluyendo "Coordinador" y "Cliente"
        private SelectList GetRolesSelectList(int? selectedRoleId)
        {
            var roles = _context.Roles.Where(r => r.Nombre != "Coordinador" && r.Nombre != "Cliente");
            return new SelectList(roles, "Id", "Nombre", selectedRoleId);
        }

        // Formulario para elegir el tipo de reporte
        public IActionResult SeleccionarReporte()
        {
            return View(); // Renderiza una vista con las opciones de reporte
        }

        // Generar reporte basado en la opción seleccionada
        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> GenerarReporte(string tipoReporte)
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).Where(u => u.Activo).ToListAsync();
            PdfDocument pdfDocument;

            switch (tipoReporte)
            {
                case "Tabla":
                    pdfDocument = CreatePdfTableDocument(usuarios);
                    break;

                case "Gráfica":
                    pdfDocument = CreatePdfChartDocument(usuarios);
                    break;

                default: // Por defecto: listado simple
                    pdfDocument = CreatePdfSimpleDocument(usuarios);
                    break;
            }

            using (var memoryStream = new MemoryStream())
            {
                pdfDocument.Save(memoryStream, false);
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Usuarios.pdf");
            }
        }

        // Método para crear el documento en formato listado simple
        [Obsolete]
        private PdfDocument CreatePdfSimpleDocument(IList<Usuario> usuarios)
        {
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

            graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue), new XRect(page.Width - 130, 20, 80, 20), XStringFormats.TopRight);

            foreach (var usuario in usuarios)
            {
                if (yPosition > page.Height - 100) // Crear nueva página si se llena
                {
                    // Crear nueva página
                    page = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    // Volver a dibujar encabezado en la nueva página
                    DrawTitle(graphics, page, fontTitle, fontSubTitle, darkBlue);

                    // Nombre de la empresa en la esquina superior derecha
                    graphics.DrawString("EPICSOFT", fontHeader, new XSolidBrush(darkBlue), new XRect(page.Width - 130, 20, 80, 20), XStringFormats.TopRight);

                    yPosition = 100; // Restablecer la posición vertical
                }

                // Dibujar cuadro estilizado para cada usuario con solo línea azul claro alrededor
                graphics.DrawRoundedRectangle(new XPen(lightBlue, 2), leftMargin - 10, yPosition - 15, page.Width - 2 * leftMargin + 10, 85, 10, 10);

                // Dibujar contenido con texto en color azul oscuro
                graphics.DrawString($"Nombre: {usuario.Nombre ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
                graphics.DrawString($"Documento: {usuario.Documento?.ToString() ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 20);
                graphics.DrawString($"Correo: {usuario.Correo ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 40);
                graphics.DrawString($"Rol: {usuario.Rol?.Nombre ?? "N/A"}", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition + 60);

                yPosition += 100; // Incrementar para el siguiente usuario
            }

            // Número de página en la parte superior derecha (Estilo APA)
            graphics.DrawString($"{pageNumber}", fontPageNumber, new XSolidBrush(darkBlue), new XRect(page.Width - 40, 20, 30, 20), XStringFormats.TopRight);

            // Pie de página con la fecha de creación del PDF
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            // Incrementar el contador de páginas
            pageNumber++;

            return pdfDocument;
        }

        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, XFont fontTitle, XFont fontSubTitle, XColor darkBlue)
        {
            graphics.DrawString("Reporte de Usuarios Activos", fontTitle, new XSolidBrush(darkBlue), new XRect(0, 30, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación", fontSubTitle, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, 60, page.Width, 20), XStringFormats.TopCenter);
        }


        // Método para crear el documento con formato tabular
        [Obsolete]
        private PdfDocument CreatePdfTableDocument(IList<Usuario> usuarios)
        {
            var pdfDocument = new PdfDocument();

            // Crear la primera página
            PdfPage page = pdfDocument.AddPage();
            XGraphics graphics = XGraphics.FromPdfPage(page);

            // Definición de fuentes y colores
            var fontTitle = new XFont("Verdana Bold", 16);
            var fontSubTitle = new XFont("Verdana", 12);
            var fontContent = new XFont("Verdana", 10);
            var fontHeader = new XFont("Verdana Bold", 10);
            var fontFooter = new XFont("Verdana Italic", 8);

            XColor headerBackgroundColor = XColor.FromArgb(60, 90, 150); // Fondo azul oscuro para encabezados
            XColor headerTextColor = XColor.FromArgb(255, 255, 255); // Texto blanco en encabezados
            XColor rowBackgroundColor1 = XColor.FromArgb(240, 248, 255); // Azul claro para filas impares
            XColor rowBackgroundColor2 = XColor.FromArgb(255, 255, 255); // Blanco para filas pares
            XColor borderColor = XColor.FromArgb(200, 200, 200); // Gris claro para bordes
            XColor textColor = XColor.FromArgb(60, 90, 150); // Azul oscuro para texto normal

            // Márgenes iniciales
            double tableWidth = 550; // Ancho total de la tabla
            double leftMargin = (page.Width - tableWidth) / 2; // Calcular margen izquierdo para centrar la tabla

            // Posición inicial vertical con ajuste hacia arriba
            double contentHeight = usuarios.Count * 25 + 60; // Altura aproximada del contenido de la tabla
            double verticalOffset = -110; // Ajuste adicional para mover la tabla más arriba
            double yPosition = Math.Max((page.Height - contentHeight) / 2 + verticalOffset, 100);

            // Dibujar título y subtítulo
            DrawTitle(graphics, page, "Reporte de Usuarios Activos", "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación", fontTitle, fontSubTitle, headerBackgroundColor, yPosition - 60);


            // Dibujar título y subtítulo
            DrawTitle(graphics, page, "Reporte de Usuarios Activos", "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación", fontTitle, fontSubTitle, headerBackgroundColor, yPosition - 60);

            // Dibujar encabezados de la tabla
            DrawTableHeader(graphics, ref yPosition, leftMargin, tableWidth, fontHeader, headerBackgroundColor, headerTextColor);

            // Dibujar las filas de la tabla
            int rowIndex = 0;
            foreach (var usuario in usuarios)
            {
                // Verificar si es necesario crear una nueva página
                if (yPosition > page.Height - 100) // Si la posición está cerca del final de la página
                {
                    // Crear nueva página
                    page = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);

                    // Dibujar encabezados en la nueva página
                    yPosition = 100; // Reiniciar posición vertical
                    DrawTitle(graphics, page, "Reporte de Usuarios Activos", "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación", fontTitle, fontSubTitle, headerBackgroundColor, yPosition - 60);
                    DrawTableHeader(graphics, ref yPosition, leftMargin, tableWidth, fontHeader, headerBackgroundColor, headerTextColor);
                }

                // Dibujar los datos de cada usuario
                DrawTableRow(graphics, usuario, ref yPosition, leftMargin, fontContent, rowIndex % 2 == 0 ? rowBackgroundColor1 : rowBackgroundColor2, textColor, borderColor, tableWidth);
                rowIndex++;
            }

            return pdfDocument;
        }

        // Método para dibujar título y subtítulo
        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, string title, string subTitle, XFont titleFont, XFont subTitleFont, XColor titleColor, double yPosition)
        {
            graphics.DrawString(title, titleFont, new XSolidBrush(titleColor), new XRect(0, yPosition, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString(subTitle, subTitleFont, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, yPosition + 30, width: page.Width, 20), XStringFormats.TopCenter);
        }

        // Método para dibujar encabezados de la tabla
        private void DrawTableHeader(XGraphics graphics, ref double yPosition, double leftMargin, double tableWidth, XFont fontHeader, XColor backgroundColor, XColor textColor)
        {
            double columnWidth1 = 150; // Ancho de la primera columna (Nombre)
            double columnWidth2 = 100; // Ancho de la segunda columna (Documento)
            double columnWidth3 = 200; // Ancho de la tercera columna (Correo)
            double columnWidth4 = 100; // Ancho de la cuarta columna (Rol)
            double headerHeight = 30;

            // Fondo del encabezado
            graphics.DrawRectangle(new XSolidBrush(backgroundColor), leftMargin, yPosition - 5, tableWidth, headerHeight);

            // Dibujar texto centrado en encabezados
            graphics.DrawString("Nombre", fontHeader, new XSolidBrush(textColor), new XRect(leftMargin, yPosition, columnWidth1, headerHeight), XStringFormats.Center);
            graphics.DrawString("Documento", fontHeader, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1, yPosition, columnWidth2, headerHeight), XStringFormats.Center);
            graphics.DrawString("Correo", fontHeader, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1 + columnWidth2, yPosition, columnWidth3, headerHeight), XStringFormats.Center);
            graphics.DrawString("Rol", fontHeader, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1 + columnWidth2 + columnWidth3, yPosition, columnWidth4, headerHeight), XStringFormats.Center);

            yPosition += headerHeight + 10; // Ajustar para las filas
        }

        // Método para dibujar filas de la tabla
        private void DrawTableRow(XGraphics graphics, Usuario usuario, ref double yPosition, double leftMargin, XFont fontContent, XColor backgroundColor, XColor textColor, XColor borderColor, double tableWidth)
        {
            double rowHeight = 25;
            double columnWidth1 = 150;
            double columnWidth2 = 100;
            double columnWidth3 = 200;
            double columnWidth4 = 100;

            // Dibujar fondo de la fila
            graphics.DrawRectangle(new XSolidBrush(backgroundColor), leftMargin, yPosition, tableWidth, rowHeight);

            // Dibujar contenido centrado
            graphics.DrawString(usuario.Nombre ?? "N/A", fontContent, new XSolidBrush(textColor), new XRect(leftMargin, yPosition, columnWidth1, rowHeight), XStringFormats.Center);
            graphics.DrawString(usuario.Documento?.ToString() ?? "N/A", fontContent, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1, yPosition, columnWidth2, rowHeight), XStringFormats.Center);
            graphics.DrawString(usuario.Correo ?? "N/A", fontContent, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1 + columnWidth2, yPosition, columnWidth3, rowHeight), XStringFormats.Center);
            graphics.DrawString(usuario.Rol?.Nombre ?? "N/A", fontContent, new XSolidBrush(textColor), new XRect(leftMargin + columnWidth1 + columnWidth2 + columnWidth3, yPosition, columnWidth4, rowHeight), XStringFormats.Center);

            yPosition += rowHeight; // Incrementar para la siguiente fila
        }


        // Método para crear el documento con gráficas
        [Obsolete]
        private PdfDocument CreatePdfChartDocument(IList<Usuario> usuarios)
        {
            var pdfDocument = new PdfDocument();
            var page = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            // Dibujar títulos
            DrawTitle(graphics, page, "Reporte de Usuarios Activos",
                "Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación",
                new XFont("Verdana Bold", 16), new XFont("Verdana", 12),
                XColor.FromArgb(60, 90, 150));

            // Dibujar gráfica
            DrawBarChart(graphics, usuarios, page);

            return pdfDocument;
        }

        // Método para dibujar el encabezado
        [Obsolete]
        private void DrawTitle(XGraphics graphics, PdfPage page, string mainTitle, string subTitle, XFont fontTitle, XFont fontSubTitle, XColor backgroundColor)
        {
            double headerHeight = 80; // Altura del encabezado
            double headerPadding = 10; // Espaciado interno

            // Dibujar fondo del encabezado
            graphics.DrawRectangle(new XSolidBrush(backgroundColor), 0, 0, page.Width, headerHeight);

            // Dibujar línea decorativa debajo del encabezado
            graphics.DrawLine(new XPen(XColor.FromArgb(100, 255, 255, 255), 2), 0, headerHeight, page.Width, headerHeight);

            // Títulos centrados
            graphics.DrawString(mainTitle, fontTitle, XBrushes.White, new XRect(0, headerPadding, page.Width, headerHeight / 2), XStringFormats.Center);
            graphics.DrawString(subTitle, fontSubTitle, new XSolidBrush(XColor.FromArgb(220, 220, 220)), new XRect(0, headerHeight / 2, page.Width, headerHeight / 2), XStringFormats.Center);
        }

        // Método para dibujar la gráfica de barras
        [Obsolete]
        private void DrawBarChart(XGraphics graphics, IList<Usuario> usuarios, PdfPage page)
        {
            var barWidth = 70;
            var maxBarHeight = 200;
            var chartHeight = 300;
            var spacing = 20;

            var usuariosPorRol = usuarios
                .GroupBy(u => u.Rol?.Nombre ?? "Sin Rol")
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToList();

            int maxCantidad = usuariosPorRol.Max(r => r.Cantidad);
            int numberOfBars = usuariosPorRol.Count;

            // Calcular ancho total de la gráfica (incluyendo barras y espacios)
            double totalChartWidth = (barWidth * numberOfBars) + (spacing * (numberOfBars - 1));
            double chartX = (page.Width - totalChartWidth) / 2; // Centrar horizontalmente
            double chartY = 120; // Ajustar posición vertical debajo del encabezado

            // Dibujar líneas del eje Y
            XColor gridColor = XColor.FromArgb(200, 200, 200);
            double gridSpacing = maxBarHeight / 5;
            for (int i = 0; i <= 5; i++)
            {
                double y = chartY + chartHeight - (gridSpacing * i);
                graphics.DrawLine(new XPen(gridColor, 1), chartX - 20, y, chartX + totalChartWidth, y);
                graphics.DrawString(((maxCantidad / 5) * i).ToString(), new XFont("Verdana", 8), XBrushes.Black, new XRect(chartX - 50, y - 10, 30, 20), XStringFormats.CenterRight);
            }

            // Dibujar barras
            int currentX = (int)chartX;
            var colors = new[] { XBrushes.SkyBlue, XBrushes.LightGreen, XBrushes.Orange, XBrushes.Purple }; // Colores para las barras
            int colorIndex = 0;

            foreach (var rolData in usuariosPorRol)
            {
                double barHeight = (rolData.Cantidad / (double)maxCantidad) * maxBarHeight;
                graphics.DrawRectangle(colors[colorIndex % colors.Length], currentX, chartY + (chartHeight - barHeight), barWidth, barHeight);

                var labelFont = new XFont("Verdana", 10);
                graphics.DrawString(rolData.Cantidad.ToString(), labelFont, XBrushes.Black, new XRect(currentX, chartY + (chartHeight - barHeight) - 20, barWidth, 20), XStringFormats.TopCenter);
                graphics.DrawString(rolData.Rol, labelFont, XBrushes.Black, new XRect(currentX, chartY + chartHeight + 10, barWidth, 20), XStringFormats.TopCenter);

                currentX += barWidth + spacing;
                colorIndex++;
            }
        }

        [Obsolete]
        public async Task<IActionResult> DescargarReporteInactivos()
        {
            var usuariosInactivos = await GetUsuariosInactivos(); // Obtener usuarios inactivos

            // Crear el documento PDF
            var pdfDocument = new PdfDocument();
            var fontTitle = new XFont("Verdana Bold", 16);
            var fontSubTitle = new XFont("Verdana", 12);
            var fontContent = new XFont("Verdana", 10);
            var fontPageNumber = new XFont("Verdana Bold", 8);
            var fontFooter = new XFont("Verdana Italic", 8);

            // Definir la página y los gráficos
            var page = pdfDocument.AddPage();
            var graphics = XGraphics.FromPdfPage(page);

            // Definir colores
            var darkBlue = XColor.FromArgb(60, 90, 150);
            var lightGray = XColor.FromArgb(220, 220, 220);

            // Título y subtítulo
            graphics.DrawString("Reporte de Usuarios Inactivos", fontTitle, new XSolidBrush(darkBlue), new XRect(0, 30, page.Width, 40), XStringFormats.TopCenter);
            graphics.DrawString("Sistema de divulgación, gestión de semilleros y proyectos digitales de investigación", fontSubTitle, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, 60, page.Width, 20), XStringFormats.TopCenter);

            double yPosition = 100;
            double leftMargin = 50;
            double rightMargin = page.Width - 50;

            // Dibujar encabezado de la tabla
            graphics.DrawLine(new XPen(darkBlue, 1), leftMargin, yPosition - 10, rightMargin, yPosition - 10);
            graphics.DrawString("Nombre", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
            graphics.DrawString("Documento", fontContent, new XSolidBrush(darkBlue), leftMargin + 150, yPosition);
            graphics.DrawString("Correo", fontContent, new XSolidBrush(darkBlue), leftMargin + 300, yPosition);
            graphics.DrawString("Fecha Inactivo", fontContent, new XSolidBrush(darkBlue), leftMargin + 450, yPosition);

            yPosition += 20;

            // Dibujar filas de usuarios inactivos
            foreach (var usuario in usuariosInactivos)
            {
                if (yPosition > page.Height - 100) // Verificar si se necesita una nueva página
                {
                    page = pdfDocument.AddPage();
                    graphics = XGraphics.FromPdfPage(page);
                    yPosition = 100;
                    // Volver a dibujar el encabezado de la tabla
                    graphics.DrawLine(new XPen(darkBlue, 1), leftMargin, yPosition - 10, rightMargin, yPosition - 10);
                    graphics.DrawString("Nombre", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
                    graphics.DrawString("Documento", fontContent, new XSolidBrush(darkBlue), leftMargin + 150, yPosition);
                    graphics.DrawString("Correo", fontContent, new XSolidBrush(darkBlue), leftMargin + 300, yPosition);
                    graphics.DrawString("Fecha Inactivo", fontContent, new XSolidBrush(darkBlue), leftMargin + 420, yPosition);
                    yPosition += 20;
                }

                // Dibujar cada fila con los datos de los usuarios inactivos
                graphics.DrawString(usuario.Nombre ?? "N/A", fontContent, new XSolidBrush(darkBlue), leftMargin, yPosition);
                graphics.DrawString(usuario.Documento?.ToString() ?? "N/A", fontContent, new XSolidBrush(darkBlue), leftMargin + 150, yPosition);
                graphics.DrawString(usuario.Correo ?? "N/A", fontContent, new XSolidBrush(darkBlue), leftMargin + 300, yPosition);
                graphics.DrawString(usuario.FechaInactivo?.ToString("dd/MM/yyyy") ?? "N/A", fontContent, new XSolidBrush(darkBlue), leftMargin + 450, yPosition);

                yPosition += 20;
            }

            // Número de página en la parte superior derecha
            graphics.DrawString("Página 1", fontPageNumber, new XSolidBrush(darkBlue), new XRect(page.Width - 40, page.Height - 30, 30, 20), XStringFormats.TopRight);

            // Pie de página con la fecha de generación
            graphics.DrawString($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}", fontFooter, new XSolidBrush(XColor.FromArgb(169, 169, 169)), new XRect(0, page.Height - 30, page.Width, 20), XStringFormats.BottomCenter);

            // Enviar el archivo PDF al usuario para descargar
            using (var memoryStream = new MemoryStream())
            {
                pdfDocument.Save(memoryStream, false); // Asegúrate de que el archivo no cierre el flujo
                memoryStream.Position = 0; // Reiniciar la posición del flujo para asegurar que se lea correctamente
                return File(memoryStream.ToArray(), "application/pdf", "Reporte_Usuarios_Inactivos.pdf"); // Descargar el archivo PDF
            }
        }

        // GET: Usuarios Activos con o sin paginación
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5; // Definir cuántos usuarios se muestran por página
            var usuarios = await GetUsuariosActivos();

            // Calcular el total de páginas
            int totalUsuarios = usuarios.Count;
            int totalPages = (int)Math.Ceiling(totalUsuarios / (double)pageSize);

            // Obtener los usuarios para la página actual
            var usuariosPagina = usuarios.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new UsuariosIndexViewModel
            {
                Usuarios = usuariosPagina,
                PageNumber = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(model);
        }



        // GET: Usuarios Inactivos
        public async Task<IActionResult> Inactivos()
        {
            var usuariosInactivos = await GetUsuariosInactivos();
            return View(usuariosInactivos);
        }

        // Obtener usuarios activos
        private async Task<IList<Usuario>> GetUsuariosActivos()
        {
            return await _context.Usuarios.Include(u => u.Rol).Where(u => u.Activo).ToListAsync();
        }

        // Obtener usuarios inactivos
        private async Task<IList<Usuario>> GetUsuariosInactivos()
        {
            return await _context.Usuarios.Include(u => u.Rol).Where(u => !u.Activo).ToListAsync();
        }

        // GET: Crear o Editar usuario
        public async Task<IActionResult> CreateOrEdit(int? id)
        {
            var usuario = id.HasValue ? await _context.Usuarios.FindAsync(id) : new Usuario();

            if (id.HasValue && usuario == null)
            {
                return NotFound();
            }

            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
            return View(usuario);
        }

        // POST: Crear o Editar usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int? id, [Bind("Id,Nombre,Documento,Correo,Contraseña,ConfirmarContraseña,RolId,Activo")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                if (id == null || id == 0) // Crear nuevo usuario
                {
                    if (string.IsNullOrEmpty(usuario.Contraseña))
                    {
                        usuario.Contraseña = "Contraseña123!";
                        usuario.ConfirmarContraseña = usuario.Contraseña;
                    }

                    if (!_authService.EsContraseñaSegura(usuario.Contraseña))
                    {
                        ModelState.AddModelError("", "La contraseña no cumple con los requisitos de seguridad.");
                        ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                        return View(usuario);
                    }

                    usuario.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                    usuario.Activo = true;
                    usuario.FechaInactivo = null;
                    _context.Add(usuario);

                    // Asignar permisos si no es Coordinador
                    AsignarPermisos(usuario);

                    // Enviar correo al nuevo usuario
                    try
                    {
                        await _emailService.SendEmailAsync(
                            usuario.Correo,
                            "Bienvenido a nuestra plataforma",
                            $"<p>Hola {usuario.Nombre},</p><p>Tu cuenta ha sido creada exitosamente. Tu contraseña inicial es: <b>Contraseña123!</b></p>"
                        );
                    }
                    catch (Exception ex)
                    {
                        // Manejar errores de correo
                        ModelState.AddModelError("", $"Error al enviar el correo: {ex.Message}");
                    }
                }
                else // Editar usuario existente
                {
                    var usuarioExistente = await _context.Usuarios.FindAsync(id);
                    if (usuarioExistente == null)
                    {
                        return NotFound();
                    }

                    usuarioExistente.Nombre = usuario.Nombre;
                    usuarioExistente.Documento = usuario.Documento;
                    usuarioExistente.Correo = usuario.Correo;
                    usuarioExistente.RolId = usuario.RolId;

                    if (!string.IsNullOrEmpty(usuario.Contraseña))
                    {
                        if (!_authService.EsContraseñaSegura(usuario.Contraseña))
                        {
                            ModelState.AddModelError("", "La contraseña no cumple con los requisitos de seguridad.");
                            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
                            return View(usuario);
                        }

                        usuarioExistente.Contraseña = _authService.ConvertirContraseña(usuario.Contraseña);
                    }

                    _context.Update(usuarioExistente);
                    AsignarPermisos(usuarioExistente);

                    // Enviar correo al usuario modificado
                    try
                    {
                        await _emailService.SendEmailAsync(
                            usuarioExistente.Correo,
                            "Actualización de cuenta",
                            $"<p>Hola {usuarioExistente.Nombre},</p><p>Tu cuenta ha sido actualizada exitosamente.</p>"
                        );
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", $"Error al enviar el correo: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Roles"] = GetRolesSelectList(usuario.RolId);
            return View(usuario);
        }

        // Asignar permisos a usuarios
        private void AsignarPermisos(Usuario usuario)
        {
            if (usuario.Rol?.Nombre != "Coordinador")
            {
                _authService.AsignarPermisosParaUsuario(_context, usuario);
            }
        }

        // Acción para Marcar Usuario como Inactivo
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = false;
            usuario.FechaInactivo = DateTime.Now;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Acción para Reactivar Usuario
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.Activo = true;
            usuario.FechaInactivo = null;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Inactivos));
        }

        // Acción para Eliminar Usuario Definitivamente
        public async Task<IActionResult> EliminarDefinitivamente(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Captura el correo del usuario antes de eliminarlo
            var correoUsuario = usuario.Correo;
            var nombreUsuario = usuario.Nombre;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            // Intentar enviar un correo de notificación
            try
            {
                await _emailService.SendEmailAsync(
                    correoUsuario,
                    "Cuenta eliminada",
                    $"<p>Hola {nombreUsuario},</p><p>Tu cuenta ha sido eliminada definitivamente del sistema. Si tienes alguna pregunta, por favor contáctanos.</p>"
                );
            }
            catch (Exception ex)
            {
                // Log o manejo del error del envío del correo
                // En un sistema más avanzado podrías registrar esto en un sistema de monitoreo/logs
                Console.WriteLine($"Error al enviar correo al usuario eliminado: {ex.Message}");
            }

            return RedirectToAction(nameof(Inactivos));
        }


        // Eliminar Usuarios Inactivos después de 30 días
        public async Task EliminarUsuariosInactivosAutomaticamente()
        {
            var fechaLimite = DateTime.Now.AddDays(-30);
            var usuariosParaEliminar = await _context.Usuarios
                .Where(u => !u.Activo && u.FechaInactivo <= fechaLimite)
                .ToListAsync();

            if (usuariosParaEliminar.Any())
            {
                _context.Usuarios.RemoveRange(usuariosParaEliminar);
                await _context.SaveChangesAsync();
            }
        }
    }
}
