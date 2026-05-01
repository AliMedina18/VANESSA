using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    /// <summary>
    /// Almacena archivos adjuntos de publicaciones de forma atómica.
    /// Reemplaza la anterior columna CSV ActividadesPublicacion.
    /// </summary>
    public class PublicacionAdjunto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Publicacion")]
        public int PublicacionId { get; set; }

        /// <summary>
        /// Relación con Publicación
        /// </summary>
        public Publicacion? Publicacion { get; set; }

        /// <summary>
        /// Ruta del archivo adjunto
        /// </summary>
        [Required]
        [StringLength(500)]
        public string RutaArchivo { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de archivo (pdf, img, video, etc.)
        /// </summary>
        [StringLength(50)]
        public string? TipoArchivo { get; set; }

        /// <summary>
        /// Orden de presentación del archivo
        /// </summary>
        public int? Orden { get; set; }

        /// <summary>
        /// Fecha de agregación del archivo
        /// </summary>
        public DateTime FechaAgregado { get; set; } = DateTime.UtcNow;
    }
}
