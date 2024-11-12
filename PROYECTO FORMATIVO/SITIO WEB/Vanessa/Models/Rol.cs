using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)] 
        public string Nombre { get; set; } = string.Empty; 

        // Relación uno a muchos con el modelo Usuario
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}
