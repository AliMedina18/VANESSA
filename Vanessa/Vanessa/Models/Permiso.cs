using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Permiso - refactorizado.
    /// Ahora con relaciones:
    /// - RolPermiso: Permisos por defecto para cada rol
    /// - UsuarioPermiso: Permisos excepcionales por usuario
    /// </summary>
    public class Permiso
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del permiso (ej: "ver_usuarios", "crear_proyecto", etc.)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Roles que tienen este permiso por defecto (relación M:M)
        /// </summary>
        public ICollection<RolPermiso>? RolPermisos { get; set; }

        /// <summary>
        /// Usuarios con este permiso (excepciones, principalmente)
        /// </summary>
        public ICollection<UsuarioPermiso>? UsuarioPermisos { get; set; }
    }
}

