using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [Range(1_000_000, 9_999_999_999, ErrorMessage = "Ingrese un número de documento válido.")]
        public int? Documento { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [StringLength(254)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8)]
        public string Contraseña { get; set; } = string.Empty;

        [Required]
        [Compare("Contraseña", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContraseña { get; set; } = string.Empty;
    }
}
