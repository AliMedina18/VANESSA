using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    public class Publicacion
    {
        [Key]
        public int Id_Publicacion { get; set; }

        [Required]
        [StringLength(50)]
        public required string NombrePublicacion { get; set; }

        [Required]
        public required string ContenidoPublicacion { get; set; }

        [Required]
        [StringLength(35)]
        public required string TipoPublicacion { get; set; }

        [Required]
        [StringLength(30)]
        public required string LugarPublicacion { get; set; }

        [Required]
        public DateTime FechaPublicacion { get; set; }

        [Required]
        public TimeSpan HoraPublicacion { get; set; }

        [Required]
        public string? ImagenPublicacion { get; set; }

        [Required]
        public required string ActividadesPublicacion { get; set; }

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public required Usuario Usuario { get; set; }
    }

}
