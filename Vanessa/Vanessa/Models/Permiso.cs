using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    public class Permiso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        // Relación muchos a muchos con Usuario
        public ICollection<UsuarioPermiso>? UsuarioPermisos { get; set; }
    }
}
