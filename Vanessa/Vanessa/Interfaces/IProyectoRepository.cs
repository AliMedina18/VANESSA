using Vanessa.Models;

namespace Vanessa.Interfaces
{
    public interface IProyectoRepository
    {
        Task<Proyecto?> GetByIdAsync(int id);
        Task<IReadOnlyList<Proyecto>> GetAllAsync();
        Task<IReadOnlyList<Proyecto>> GetByUsuarioIdAsync(int usuarioId);
        Task AddAsync(Proyecto proyecto);
        Task UpdateAsync(Proyecto proyecto);
        Task DeleteAsync(Proyecto proyecto);
        Task<bool> ExistsAsync(int id);
    }
}
