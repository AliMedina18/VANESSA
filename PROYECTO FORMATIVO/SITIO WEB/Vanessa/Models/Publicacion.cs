using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    public class Publicacion
    {
        [Key]
        public int? Id_Publicacion { get; set; }

        public string? NombrePublicacion { get; set; }

        public string? ContenidoPublicacion { get; set; }

        public string? TipoPublicacion { get; set; }

        public string? LugarPublicacion { get; set; }

        public DateTime FechaPublicacion { get; set; }

        public TimeSpan HoraPublicacion { get; set; }

        public string? ImagenPublicacion { get; set; }

        public string? ActividadesPublicacion { get; set; }

        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
    }

}