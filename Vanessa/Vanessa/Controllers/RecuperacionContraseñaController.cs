using Microsoft.AspNetCore.Mvc;
using Vanessa.Models;
using Vanessa.Services;

namespace Vanessa.Controllers
{
    public class RecuperacionContraseñaController : Controller
    {
        private readonly RecuperacionService _recuperacionService;

        public RecuperacionContraseñaController(RecuperacionService recuperacionService)
        {
            _recuperacionService = recuperacionService;
        }

        [HttpGet]
        public IActionResult Solicitar() => View();

        [HttpPost]
        public async Task<IActionResult> Solicitar(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                ModelState.AddModelError("", "Por favor ingrese su correo.");
                return View();
            }

            var resultado = await _recuperacionService.SolicitarRecuperacionAsync(correo, Url);
            if (resultado)
                return View("CorreoEnviado");

            ModelState.AddModelError("", "No se encontró un usuario con ese correo.");
            return View();
        }

        [HttpGet]
        public IActionResult Restablecer(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

            return View(new RestablecerContraseñaViewModel
            {
                Token             = token,
                NuevaContraseña   = string.Empty,
                ConfirmarContraseña = string.Empty
            });
        }

        [HttpPost]
        public IActionResult Restablecer(RestablecerContraseñaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_recuperacionService.RestablecerContraseña(model.Token, model.NuevaContraseña))
                return RedirectToAction("Login", "Auth");

            ModelState.AddModelError("", "El token es inválido o ha expirado.");
            return View(model);
        }
    }
}
