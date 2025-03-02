using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    public class Proyecto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? DocumentoProyecto { get; set; }
        public string? EquiposInvestigacion { get; set; }
        public DateTime FechaInicio { get; set; }

        // Llave foránea hacia Semillero
        [Required]
        public int SemilleroId { get; set; }
        [ForeignKey("SemilleroId")]
        public Semillero? Semillero { get; set; }

        // Llave foránea hacia Usuario (Dueño del proyecto)
        [Required]
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
