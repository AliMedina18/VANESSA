using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Vanessa.Models.Enums;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Proyecto - refactorizado para 3FN.
    /// - EquiposInvestigacion (CSV) → Tabla separada ProyectoMiembro
    /// - UsuarioId → UsuarioCoordenadorId (semántica clara)
    /// - Auditoría: FechaCreacion, FechaActualizacion
    /// </summary>
    public class Proyecto
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del proyecto
        /// </summary>
        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 200 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del archivo PDF del documento principal del proyecto
        /// </summary>
        [StringLength(260)]
        public string? DocumentoProyecto { get; set; }

        /// <summary>
        /// Fecha de inicio del proyecto
        /// </summary>
        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        /// <summary>
        /// Fecha de finalización prevista u oficial (nullable hasta conclusión)
        /// </summary>
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Estado del ciclo de vida del proyecto (enum: Activo, Pausado, Finalizado, etc.)
        /// </summary>
        public EstadoProyecto Estado { get; set; } = EstadoProyecto.Activo;

        /// <summary>
        /// Semillero al que pertenece el proyecto (FK)
        /// </summary>
        [Required]
        [ForeignKey("Semillero")]
        public int SemilleroId { get; set; }
        public Semillero? Semillero { get; set; }

        /// <summary>
        /// Usuario coordinador/responsable del proyecto (FK)
        /// Antes: UsuarioId (ambiguo)
        /// </summary>
        [Required]
        [ForeignKey("UsuarioCoordenador")]
        public int UsuarioCoordenadorId { get; set; }
        public Usuario? UsuarioCoordenador { get; set; }

        /// <summary>
        /// Miembros del proyecto con sus roles (relación 1:N con ProyectoMiembro)
        /// Reemplaza anterior EquiposInvestigacion (CSV)
        /// </summary>
        public ICollection<ProyectoMiembro> Miembros { get; set; } = new List<ProyectoMiembro>();

        [NotMapped]
        public string? EquiposInvestigacion
        {
            get => Miembros == null || Miembros.Count == 0
                ? string.Empty
                : string.Join(", ", Miembros.Select(m => m.Usuario?.Nombre ?? string.Empty));
            set { /* compatibilidad con vistas legacy; la relación real usa Miembros */ }
        }

        /// <summary>
        /// Fecha de creación del proyecto (auditoría)
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización (auditoría)
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    }
}

