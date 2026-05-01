using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models.ViewModels
{
    public class EditarPerfilViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        [StringLength(254)]
        public string Correo { get; set; } = string.Empty;

        // Opcional: solo se actualiza si se provee
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string? Contraseña { get; set; }

        [Compare("Contraseña", ErrorMessage = "La confirmación de contraseña no coincide.")]
        public string? ConfirmarContraseña { get; set; }
    }
}
