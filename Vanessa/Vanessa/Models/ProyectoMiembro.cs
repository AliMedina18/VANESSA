using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Relación muchos-a-muchos entre Proyecto y Usuario.
    /// Define el rol de cada usuario en un proyecto y auditoría de participación.
    /// Reemplaza la anterior columna de texto EquiposInvestigacion.
    /// </summary>
    public class ProyectoMiembro
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Proyecto")]
        public int ProyectoId { get; set; }

        /// <summary>
        /// Relación con Proyecto
        /// </summary>
        public Proyecto? Proyecto { get; set; }

        [Required]
        [ForeignKey("Usuario")]
        public int UsuarioId { get; set; }

        /// <summary>
        /// Relación con Usuario
        /// </summary>
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Rol del usuario en el proyecto: coordinador, investigador, asistente
        /// </summary>
        [Required]
        [StringLength(50)]
        public string RolMiembro { get; set; } = "investigador"; // Default: investigador

        /// <summary>
        /// Fecha en que se incorporó el usuario al proyecto
        /// </summary>
        public DateTime FechaIncorporacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha en que se desvinculó (null si aún activo)
        /// </summary>
        public DateTime? FechaDesvinculacion { get; set; }
    }
}
