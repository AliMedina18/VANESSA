using Microsoft.AspNetCore.Mvc;
using Vanessa.Services;
using Vanessa.Models;
using System.Threading.Tasks;

namespace Vanessa.Controllers
{
    public class RecuperacionContraseñaController : Controller
    {
        private readonly RecuperacionService _recuperacionService;

        public RecuperacionContraseñaController(RecuperacionService recuperacionService)
        {
            _recuperacionService = recuperacionService;
        }

        // Vista para solicitar la recuperación de contraseña
        [HttpGet]
        public IActionResult Solicitar()
        {
            return View();
        }

        // Acción que se llama cuando el usuario envía el correo
        [HttpPost]
        public async Task<IActionResult> Solicitar(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                ModelState.AddModelError("", "Por favor ingrese su correo.");
                return View();
            }

            // Pasar el `Url` helper al servicio para generar la URL dinámica
            var resultado = await _recuperacionService.SolicitarRecuperacionAsync(correo, Url);
            if (resultado)
            {
                return View("CorreoEnviado"); // Vista que indica que el correo ha sido enviado
            }
            else
            {
                ModelState.AddModelError("", "No se encontró un usuario con ese correo.");
            }

            return View();
        }

        // Vista para restablecer la contraseña
        [HttpGet]
        public IActionResult Restablecer(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home"); // Redirigir a la página principal si no hay token

            // Pasar el token al modelo de la vista
            return View(model: new RestablecerContraseñaViewModel { Token = token });
        }

        // Acción que se llama cuando el usuario envía la nueva contraseña
        [HttpPost]
        public IActionResult Restablecer(RestablecerContraseñaViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Intentar restablecer la contraseña con el token y la nueva contraseña
                var resultado = _recuperacionService.RestablecerContraseña(model.Token, model.NuevaContraseña);
                if (resultado)
                {
                    return RedirectToAction("Login", "Auth"); // Redirigir al login si el restablecimiento fue exitoso
                }
                else
                {
                    ModelState.AddModelError("", "El token es inválido o ha expirado.");
                }
            }

            return View(model); // Mostrar la vista con los errores
        }
    }
}
