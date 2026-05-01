using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _context;

        public UsuarioRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Usuario?> GetByIdAsync(int id) =>
            _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

        public Task<Usuario?> GetByDocumentoAsync(long documento) =>
            _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Documento == documento);

        public Task<Usuario?> GetByCorreoAsync(string correo) =>
            _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo && u.Activo);

        public async Task<IReadOnlyList<Usuario>> GetAllActiveAsync() =>
            await _context.Usuarios.Include(u => u.Rol).Where(u => u.Activo).ToListAsync();

        public async Task<IReadOnlyList<Usuario>> GetAllInactiveAsync() =>
            await _context.Usuarios.Include(u => u.Rol).Where(u => !u.Activo).ToListAsync();

        public async Task AddAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public Task<bool> ExistsByDocumentoAsync(long documento) =>
            _context.Usuarios.AnyAsync(u => u.Documento == documento);

        public Task<bool> ExistsByCorreoAsync(string correo) =>
            _context.Usuarios.AnyAsync(u => u.Correo == correo);

        public IQueryable<Usuario> Query() =>
            _context.Usuarios.Include(u => u.Rol);

        public async Task AsignarPermisosClienteAsync(int usuarioId)
        {
            var permisosCliente = await _context.Permisos
                .Where(p => p.Nombre.Contains("Cliente") ||
                            p.Nombre == "VerPerfilPropio" ||
                            p.Nombre == "ActualizarPerfilPropio")
                .ToListAsync();

            foreach (var permiso in permisosCliente)
            {
                bool yaExiste = await _context.UsuarioPermisos
                    .AnyAsync(up => up.UsuarioId == usuarioId && up.PermisoId == permiso.Id);

                if (!yaExiste)
                {
                    _context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuarioId,
                        PermisoId = permiso.Id
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
