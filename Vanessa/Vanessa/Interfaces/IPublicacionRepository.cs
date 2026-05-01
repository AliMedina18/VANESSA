using Vanessa.Models;

namespace Vanessa.Interfaces
{
    public interface IPublicacionRepository
    {
        Task<Publicacion?> GetByIdAsync(int id);
        Task<Publicacion?> GetByIdWithUsuarioAsync(int id);
        IQueryable<Publicacion> QueryWithUsuario();
        Task<IReadOnlyList<Publicacion>> GetByUsuarioIdAsync(int usuarioId);
        Task AddAsync(Publicacion publicacion);
        Task UpdateAsync(Publicacion publicacion);
        Task DeleteAsync(Publicacion publicacion);
    }
}
