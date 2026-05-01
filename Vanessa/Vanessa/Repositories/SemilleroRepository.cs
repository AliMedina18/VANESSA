using Microsoft.EntityFrameworkCore;
using Vanessa.Data;
using Vanessa.Interfaces;
using Vanessa.Models;

namespace Vanessa.Repositories
{
    public class SemilleroRepository : ISemilleroRepository
    {
        private readonly ApplicationDbContext _context;

        public SemilleroRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Semillero?> GetByIdAsync(int id) =>
            _context.Semilleros.FirstOrDefaultAsync(s => s.Id == id);

        public Task<Semillero?> GetByIdWithProyectosAsync(int id) =>
            _context.Semilleros.Include(s => s.Proyectos).FirstOrDefaultAsync(s => s.Id == id);

        public async Task<IReadOnlyList<Semillero>> GetAllAsync() =>
            await _context.Semilleros.ToListAsync();

        public async Task AddAsync(Semillero semillero)
        {
            _context.Semilleros.Add(semillero);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Semillero semillero)
        {
            _context.Semilleros.Update(semillero);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Semillero semillero)
        {
            _context.Semilleros.Remove(semillero);
            await _context.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(int id) =>
            _context.Semilleros.AnyAsync(s => s.Id == id);
    }
}
