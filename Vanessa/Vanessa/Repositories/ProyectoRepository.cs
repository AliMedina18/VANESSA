using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Repositories
{
    public class ProyectoRepository : IProyectoRepository
    {
        private readonly ApplicationDbContext _context;

        public ProyectoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Proyecto?> GetByIdAsync(int id) =>
            _context.Proyectos
                .Include(p => p.Miembros)
                    .ThenInclude(pm => pm.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IReadOnlyList<Proyecto>> GetAllAsync() =>
            await _context.Proyectos.OrderByDescending(p => p.FechaInicio).ToListAsync();

        public async Task<IReadOnlyList<Proyecto>> GetByUsuarioIdAsync(int usuarioId) =>
            await _context.Proyectos
                .Where(p => EF.Property<int>(p, "UsuarioCoordinadorId") == usuarioId)
                .ToListAsync();

        public async Task AddAsync(Proyecto proyecto)
        {
            _context.Proyectos.Add(proyecto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Proyecto proyecto)
        {
            _context.Proyectos.Update(proyecto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Proyecto proyecto)
        {
            _context.Proyectos.Remove(proyecto);
            await _context.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(int id) =>
            _context.Proyectos.AnyAsync(p => p.Id == id);
    }
}
