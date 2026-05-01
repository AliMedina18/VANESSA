using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Define permisos por defecto para cada rol (relación muchos-a-muchos).
    /// Esto establece qué permisos tiene cada rol en el sistema.
    /// UsuarioPermiso complementa esta tabla para permisos excepcionales por usuario.
    /// </summary>
    public class RolPermiso
    {
        [Required]
        [ForeignKey("Rol")]
        public int RolId { get; set; }

        /// <summary>
        /// Relación con Rol
        /// </summary>
        public Rol? Rol { get; set; }

        [Required]
        [ForeignKey("Permiso")]
        public int PermisoId { get; set; }

        /// <summary>
        /// Relación con Permiso
        /// </summary>
        public Permiso? Permiso { get; set; }
    }
}
