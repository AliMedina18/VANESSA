using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Rol - refactorizado.
    /// Ahora incluye relación con RolPermiso para definir permisos por rol.
    /// </summary>
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del rol (ej: "Coordinador", "Investigador", "Admin", etc.)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Usuarios con este rol
        /// </summary>
        public ICollection<Usuario>? Usuarios { get; set; }

        /// <summary>
        /// Permisos por defecto para este rol (relación M:M)
        /// </summary>
        public ICollection<RolPermiso>? Permisos { get; set; }
    }
}

