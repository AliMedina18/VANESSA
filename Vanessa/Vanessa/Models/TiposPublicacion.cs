using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    /// <summary>
    /// Tabla de tipos de publicación (enum table).
    /// Valores: articulo, evento, noticia
    /// </summary>
    public class TiposPublicacion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty; // articulo, evento, noticia

        [StringLength(255)]
        public string? Descripcion { get; set; }

        // Relación con Publicaciones
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
    }
}
