using System.ComponentModel.DataAnnotations;

namespace Vanessa.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El documento es obligatorio.")]
        public int Documento { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contraseña { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public int RolId { get; set; }
    }
}
