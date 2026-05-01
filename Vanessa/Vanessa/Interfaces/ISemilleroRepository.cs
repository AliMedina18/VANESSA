using Vanessa.Models;

namespace Vanessa.Interfaces
{
    public interface ISemilleroRepository
    {
        Task<Semillero?> GetByIdAsync(int id);
        Task<Semillero?> GetByIdWithProyectosAsync(int id);
        Task<IReadOnlyList<Semillero>> GetAllAsync();
        Task AddAsync(Semillero semillero);
        Task UpdateAsync(Semillero semillero);
        Task DeleteAsync(Semillero semillero);
        Task<bool> ExistsAsync(int id);
    }
}
