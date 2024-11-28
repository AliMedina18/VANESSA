using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models
{
    public class RestablecerContraseñaViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public  string NuevaContraseña { get; set; }

        [Required]
        [Compare("NuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public  string ConfirmarContraseña { get; set; }

        public string Token { get; set; }
    }
}
