using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Data
{
    /// <summary>
    /// Inicializa la base de datos con datos semilla.
    /// Las credenciales del coordinador se leen desde SeedSettings
    /// (appsettings.Development.json o variables de entorno).
    /// </summary>
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(
            ApplicationDbContext context,
            IAuthService authService,
            IConfiguration config,
            ILogger<DbInitializer> logger)
        {
            _context     = context;
            _authService = authService;
            _config      = config;
            _logger      = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                if (_context.Roles.Any() && _context.Permisos.Any())
                    return;

                await SeedRolesAsync();
                await SeedPermisosAsync();
                await SeedTiposPublicacionAsync();
                await SeedCoordinadorAsync();
                await SeedUsuariosRegularesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el seeding de la base de datos.");
            }
        }

        private async Task SeedRolesAsync()
        {
            if (_context.Roles.Any()) return;
            _context.Roles.AddRange(
                new Rol { Nombre = "Coordinador" },
                new Rol { Nombre = "Cliente" },
                new Rol { Nombre = "Docente" },
                new Rol { Nombre = "Estudiante" }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedPermisosAsync()
        {
            if (_context.Permisos.Any()) return;
            _context.Permisos.AddRange(
                new Permiso { Nombre = "VerUsuarios",                  Descripcion = "Puede ver la lista de usuarios" },
                new Permiso { Nombre = "CrearUsuarios",                Descripcion = "Puede crear nuevos usuarios" },
                new Permiso { Nombre = "EditarUsuarios",               Descripcion = "Puede editar usuarios existentes" },
                new Permiso { Nombre = "EliminarUsuarios",             Descripcion = "Puede eliminar usuarios" },
                new Permiso { Nombre = "VerPerfilPropio",              Descripcion = "Puede ver su propio perfil" },
                new Permiso { Nombre = "ActualizarPerfilPropio",       Descripcion = "Puede actualizar su propio perfil" },
                new Permiso { Nombre = "EliminarPerfilPropioConPermiso", Descripcion = "Puede eliminar su cuenta con aprobación" },
                new Permiso { Nombre = "SuperUsuario",                 Descripcion = "Acceso total" },
                new Permiso { Nombre = "CrearProyectos",               Descripcion = "Puede crear proyectos" },
                new Permiso { Nombre = "EditarProyectos",              Descripcion = "Puede editar proyectos" },
                new Permiso { Nombre = "EliminarProyectos",            Descripcion = "Puede eliminar proyectos" },
                new Permiso { Nombre = "EditarSemillero",              Descripcion = "Puede editar semilleros" },
                new Permiso { Nombre = "VerProyectos",                 Descripcion = "Puede ver proyectos" },
                new Permiso { Nombre = "EditarPublicaciones",          Descripcion = "Puede editar cualquier publicación" }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedTiposPublicacionAsync()
        {
            if (_context.TiposPublicacion.Any()) return;

            _context.TiposPublicacion.AddRange(
                new TiposPublicacion { Id = 1, Nombre = "articulo", Descripcion = "Artículo académico o de opinión." },
                new TiposPublicacion { Id = 2, Nombre = "evento", Descripcion = "Publicación de evento o convocatoria." },
                new TiposPublicacion { Id = 3, Nombre = "noticia", Descripcion = "Noticia breve o comunicado." }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedCoordinadorAsync()
        {
            // Las credenciales vienen de configuración, nunca hardcodeadas
            var correo   = _config["SeedSettings:CoordinadorCorreo"]  ?? string.Empty;
            var password = _config["SeedSettings:CoordinadorPassword"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("SeedSettings:CoordinadorCorreo o Password no configurados. Saltando seed de coordinador.");
                return;
            }

            if (_context.Usuarios.Any(u => u.Correo == correo)) return;

            var rol = _context.Roles.FirstOrDefault(r => r.Nombre == "Coordinador");
            if (rol == null) return;

            var usuario = new Usuario
            {
                Nombre     = "Coordinador",
                Documento  = 12345678,
                Correo     = correo,
                Contraseña = _authService.ConvertirContraseña(password),
                RolId      = rol.Id,
                Activo     = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            await AsignarTodosPermisosAsync(usuario);
        }

        private async Task SeedUsuariosRegularesAsync()
        {
            await SeedUsuarioAsync("Cliente",     "cliente@correo.com",    "Pass*1234",       new[] { "VerProyectos" });
            await SeedUsuarioAsync("Docente",     "docente@correo.com",    "Docente*1234",    new[] { "CrearProyectos", "EditarProyectos", "VerProyectos" });
            await SeedUsuarioAsync("Estudiante",  "estudiante@correo.com", "Estudiante*1234", new[] { "CrearProyectos", "VerProyectos" });
        }

        private async Task SeedUsuarioAsync(string rolNombre, string correo, string password, string[] permisos)
        {
            if (_context.Usuarios.Any(u => u.Correo == correo)) return;
            var rol = _context.Roles.FirstOrDefault(r => r.Nombre == rolNombre);
            if (rol == null) return;

            var usuario = new Usuario
            {
                Nombre     = rolNombre + " Demo",
                Documento  = new Random().Next(10000000, 99999999),
                Correo     = correo,
                Contraseña = _authService.ConvertirContraseña(password),
                RolId      = rol.Id,
                Activo     = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var listaPermisos = _context.Permisos.Where(p => permisos.Contains(p.Nombre)).ToList();
            foreach (var p in listaPermisos)
                _context.UsuarioPermisos.Add(new UsuarioPermiso { UsuarioId = usuario.Id, PermisoId = p.Id });
            await _context.SaveChangesAsync();
        }

        private async Task AsignarTodosPermisosAsync(Usuario usuario)
        {
            foreach (var p in _context.Permisos.ToList())
                _context.UsuarioPermisos.Add(new UsuarioPermiso { UsuarioId = usuario.Id, PermisoId = p.Id });
            await _context.SaveChangesAsync();
        }
    }
}
