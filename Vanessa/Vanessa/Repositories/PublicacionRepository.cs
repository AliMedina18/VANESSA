using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Repositories
{
    public class PublicacionRepository : IPublicacionRepository
    {
        private readonly ApplicationDbContext _context;

        public PublicacionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Publicacion?> GetByIdAsync(int id) =>
            _context.Publicaciones.FirstOrDefaultAsync(p => p.Id == id);

        public Task<Publicacion?> GetByIdWithUsuarioAsync(int id) =>
            _context.Publicaciones
                .Include(p => p.Usuario)
                .Include(p => p.TipoPublicacion)
                .FirstOrDefaultAsync(p => p.Id == id);

        public IQueryable<Publicacion> QueryWithUsuario() =>
            _context.Publicaciones
                .Include(p => p.Usuario)
                .Include(p => p.TipoPublicacion);

        public async Task<IReadOnlyList<Publicacion>> GetByUsuarioIdAsync(int usuarioId) =>
            await _context.Publicaciones
                .Include(p => p.TipoPublicacion)
                .Where(p => p.UsuarioId == usuarioId)
                .ToListAsync();

        public async Task AddAsync(Publicacion publicacion)
        {
            _context.Publicaciones.Add(publicacion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Publicacion publicacion)
        {
            _context.Publicaciones.Update(publicacion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Publicacion publicacion)
        {
            _context.Publicaciones.Remove(publicacion);
            await _context.SaveChangesAsync();
        }
    }
}
