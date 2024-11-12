using Vanessa.Data;
using Vanessa.Models;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Vanessa.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para convertir la contraseña a un hash con salt utilizando PBKDF2
        public string ConvertirContraseña(string contraseña)
        {
            byte[] salt = new byte[128 / 8]; // 128 bits para el salt
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt); // Llena el salt con valores aleatorios
            }

            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        // Método para verificar si la contraseña proporcionada es correcta comparando el hash
        public bool VerificarContraseña(string hashCompleto, string contraseña)
        {
            var parts = hashCompleto.Split('.');
            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            var hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hashedInput == hash;
        }

        // Método para verificar si una contraseña es segura
        public bool EsContraseñaSegura(string contraseña)
        {
            return contraseña.Length >= 8 &&
                   contraseña.Any(char.IsDigit) &&
                   contraseña.Any(char.IsUpper) &&
                   contraseña.Any(char.IsLower) &&
                   contraseña.Any(ch => !char.IsLetterOrDigit(ch));
        }

        // Método para asignar permisos a usuarios regulares (no Coordinadores)
        public void AsignarPermisosParaUsuario(ApplicationDbContext context, Usuario usuario)
        {
            if (usuario.Rol.Nombre != "Coordinador")
            {
                // Asignamos permisos predeterminados solo si el usuario NO es Coordinador
                var permisos = context.Permisos.Where(p => p.Nombre == "VerPerfilPropio" || p.Nombre == "ActualizarPerfilPropio").ToList();

                foreach (var permiso in permisos)
                {
                    context.UsuarioPermisos.Add(new UsuarioPermiso
                    {
                        UsuarioId = usuario.Id,
                        PermisoId = permiso.Id
                    });
                }
                context.SaveChanges();
            }
        }
    }
}
