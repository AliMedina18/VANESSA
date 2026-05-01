using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Semillero - refactorizado para 3FN.
    /// - FK semanticamente clara: UsuarioCoordinadorId
    /// - Auditoría: FechaCreacion, FechaActualizacion
    /// - Soft-delete: Activo
    /// </summary>
    public class Semillero
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del semillero
        /// </summary>
        [Required(ErrorMessage = "El nombre del semillero es obligatorio.")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del archivo de imagen del semillero
        /// </summary>
        [StringLength(260)]
        public string? Imagen { get; set; }

        /// <summary>
        /// Área de investigación del semillero
        /// </summary>
        [StringLength(100)]
        public string? Area { get; set; }

        /// <summary>
        /// Descripción del semillero y sus objetivos
        /// </summary>
        [StringLength(2000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Usuario coordinador/responsable del semillero (FK)
        /// Antes: UsuarioId (ambiguo)
        /// </summary>
        [Required]
        [ForeignKey("UsuarioCoordinador")]
        public int UsuarioCoordinadorId { get; set; }
        public Usuario? UsuarioCoordinador { get; set; }

        /// <summary>
        /// Estado del semillero (true=activo, false=inactivo/eliminado)
        /// Soft-delete para mantener historial
        /// </summary>
        public bool Activo { get; set; } = true;

        /// <summary>
        /// Fecha de creación del semillero (auditoría)
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización (auditoría)
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Proyectos asociados a este semillero
        /// </summary>
        public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
    }
}

