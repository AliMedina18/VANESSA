namespace Vanessa.Models
{
    public class UsuarioPermiso
    {
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!; 

        public int PermisoId { get; set; }
        public Permiso Permiso { get; set; } = null!;
    }
}
