using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    public class RestablecerContraseñaViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public required string NuevaContraseña { get; set; }

        [Required]
        [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public required string ConfirmarContraseña { get; set; }

        public required string Token { get; set; }

        // Constructor para inicializar valores predeterminados
        public RestablecerContraseñaViewModel()
        {
            NuevaContraseña = string.Empty;
            ConfirmarContraseña = string.Empty;
            Token = string.Empty;
        }
    }

}
