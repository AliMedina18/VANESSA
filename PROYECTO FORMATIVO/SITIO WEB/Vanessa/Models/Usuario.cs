using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanessa.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(40)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int? Documento { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Contraseña { get; set; } = string.Empty;

        [NotMapped]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContraseña { get; set; } = string.Empty;

        [ForeignKey("Rol")]
        public int RolId { get; set; }

        public Rol? Rol { get; set; }

        // Relación muchos a muchos con permisos
        public ICollection<UsuarioPermiso>? UsuarioPermisos { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime? FechaInactivo { get; set; }

        // Relación uno a muchos con Semillero
        public ICollection<Semillero> Semilleros { get; set; } = new List<Semillero>();

        // Relación uno a muchos con Publicacion
        public ICollection<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();

        // Nuevas propiedades para el token de recuperación
        public string? TokenRecuperacion { get; set; }  // El token de recuperación
        public DateTime? TokenExpiracion { get; set; }  // La fecha de expiración del token
    }
}

