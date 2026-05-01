using Vanessa.Models;

namespace Vanessa.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByDocumentoAsync(long documento);
        Task<Usuario?> GetByCorreoAsync(string correo);
        Task<IReadOnlyList<Usuario>> GetAllActiveAsync();
        Task<IReadOnlyList<Usuario>> GetAllInactiveAsync();
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(Usuario usuario);
        Task<bool> ExistsByDocumentoAsync(long documento);
        Task<bool> ExistsByCorreoAsync(string correo);
        Task AsignarPermisosClienteAsync(int usuarioId);
        IQueryable<Usuario> Query();
    }
}
