using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Vanessa.Models
{
    /// <summary>
    /// Modelo de Publicación - refactorizado para 3FN.
    /// - Tipo de publicación es FK a tabla TiposPublicacion (no string)
    /// - Archivos adjuntos en tabla separada PublicacionAdjunto (no CSV)
    /// - HoraPublicacion eliminado (derivado de FechaPublicacion)
    /// - Propiedades legacy expuestas como not mapped para compatibilidad con vistas existentes.
    /// </summary>
    public class Publicacion
    {
        [Key]
        [Column("Id")]  // Antes: Id_Publicacion (inconsistencia)
        public int Id { get; set; }

        /// <summary>
        /// Título de la publicación
        /// </summary>
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        /// <summary>
        /// Contenido de la publicación
        /// </summary>
        [StringLength(5000)]
        public string? Contenido { get; set; }

        /// <summary>
        /// Tipo de publicación (FK a tabla TiposPublicacion)
        /// Valores: articulo, evento, noticia
        /// </summary>
        [Required]
        [ForeignKey("TipoPublicacion")]
        public int TipoPublicacionId { get; set; }
        public TiposPublicacion? TipoPublicacion { get; set; }

        /// <summary>
        /// Lugar donde ocurre/publicación (aplica para eventos, artículos)
        /// </summary>
        [StringLength(200)]
        public string? LugarPublicacion { get; set; }

        /// <summary>
        /// Fecha y hora (UTC) de la publicación. Incluye ambos datos.
        /// HoraPublicacion fue eliminado por redundancia.
        /// </summary>
        [Required]
        public DateTime FechaPublicacion { get; set; }

        /// <summary>
        /// Imagen destacada de la publicación
        /// </summary>
        [StringLength(500)]
        public string? Imagen { get; set; }

        /// <summary>
        /// Archivos adjuntos de la publicación (relación 1:N con PublicacionAdjunto)
        /// Reemplaza anterior ActividadesPublicacion (CSV)
        /// </summary>
        public ICollection<PublicacionAdjunto> Adjuntos { get; set; } = new List<PublicacionAdjunto>();

        /// <summary>
        /// Usuario que creó la publicación (FK)
        /// </summary>
        public int? UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// Fecha de creación de la publicación (auditoría)
        /// </summary>
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización (auditoría)
        /// </summary>
        public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string? NombrePublicacion
        {
            get => Titulo;
            set
            {
                if (value != null)
                    Titulo = value;
            }
        }

        [NotMapped]
        public string? ContenidoPublicacion
        {
            get => Contenido;
            set => Contenido = value;
        }

        [NotMapped]
        public string? ImagenPublicacion
        {
            get => Imagen;
            set => Imagen = value;
        }

        [NotMapped]
        public string? TipoPublicacionTexto
        {
            get => TipoPublicacion?.Nombre;
            set { /* compatibilidad; mapping se realiza en controller */ }
        }

        [NotMapped]
        public string? ActividadesPublicacion
        {
            get => Adjuntos == null || Adjuntos.Count == 0
                ? null
                : string.Join(",", Adjuntos.OrderBy(a => a.Orden).Select(a => a.RutaArchivo));
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                var archivos = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Adjuntos.Clear();
                var orden = 1;
                foreach (var archivo in archivos)
                {
                    Adjuntos.Add(new PublicacionAdjunto
                    {
                        RutaArchivo = archivo.Trim(),
                        Orden = orden++
                    });
                }
            }
        }

        // ─── PROPIEDADES CALCULADAS (NO MAPEADAS) ──────────────────────
        /// <summary>
        /// Hora de publicación derivada de FechaPublicacion (no persiste en BD)
        /// </summary>
        [NotMapped]
        public string HoraTexto => FechaPublicacion.ToString("HH:mm");

        /// <summary>
        /// Archivos adjuntos como lista de rutas individuales (para compatibilidad)
        /// </summary>
        [NotMapped]
        public IReadOnlyList<string> AdjuntosRutas =>
            Adjuntos == null || Adjuntos.Count == 0
                ? Array.Empty<string>()
                : Adjuntos.OrderBy(a => a.Orden).Select(a => a.RutaArchivo).ToList();
    }
}

