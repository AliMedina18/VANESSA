using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Relación muchos-a-muchos entre Usuario y Permiso.
    /// Define permisos excepcionales (agregados o denegados) para usuarios específicos.
    /// RolPermiso complementa esta tabla para permisos por defecto de roles.
    /// </summary>
    public class UsuarioPermiso
    {
        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        [Required]
        [ForeignKey("Permiso")]
        public int PermisoId { get; set; }
        public Permiso Permiso { get; set; } = null!;

        /// <summary>
        /// Tipo de permiso: ALLOW (otorgado) o DENY (denegado)
        /// </summary>
        [Required]
        [StringLength(10)]
        public string Tipo { get; set; } = "ALLOW";  // Default: otorgado

        /// <summary>
        /// Fecha de otorgamiento del permiso (auditoría)
        /// </summary>
        public DateTime FechaOtorgamiento { get; set; } = DateTime.UtcNow;
    }
}

