using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    public class Publicacion
    {
        [Key]
        public int? Id_Publicacion { get; set; }

        [Required(ErrorMessage = "El nombre de la publicación es obligatorio")]
        [Display(Name = "Nombre de la Publicación")]
        public string NombrePublicacion { get; set; }

        [Required(ErrorMessage = "El contenido de la publicación es obligatorio")]
        [Display(Name = "Contenido")]
        public string ContenidoPublicacion { get; set; }

        [Display(Name = "Tipo de Publicación")]
        public string? TipoPublicacion { get; set; }

        [Display(Name = "Lugar de Publicación")]
        public string? LugarPublicacion { get; set; }

        [Required(ErrorMessage = "La fecha de publicación es obligatoria")]
        [Display(Name = "Fecha de Publicación")]
        [DataType(DataType.Date)]
        public DateTime FechaPublicacion { get; set; }

        [Display(Name = "Hora de Publicación")]
        [DataType(DataType.Time)]
        public TimeSpan HoraPublicacion { get; set; }

        [Display(Name = "Imagen")]
        public string? ImagenPublicacion { get; set; }

        [Display(Name = "Actividades de la Publicación")]
        public string? ActividadesPublicacion { get; set; }

        // Propiedades transitorias para manejar la carga de archivos
        [NotMapped]
        [Display(Name = "Archivo de Imagen")]
        public IFormFile? ImagenArchivo { get; set; }
    }
}