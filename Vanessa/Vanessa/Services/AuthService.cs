using Vanessa.Data;
using Vanessa.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Vanessa.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        // Constantes para la configuración de PBKDF2
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int IterationCount = 10000;

        public AuthService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <param name="contraseña">La contraseña en texto plano.</param>
        /// <returns>Una cadena con el salt y el hash concatenados.</returns>
        public string ConvertirContraseña(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(contraseña));

            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt); // Genera el salt de forma segura

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        /// <param name="hashCompleto">El hash completo con salt (formato salt.hash).</param>
        /// <param name="contraseña">La contraseña ingresada por el usuario.</param>
        /// <returns>True si la contraseña es correcta, false en caso contrario.</returns>
        public bool VerificarContraseña(string hashCompleto, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(hashCompleto) || string.IsNullOrWhiteSpace(contraseña))
                return false;

            var parts = hashCompleto.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hash = parts[1];

            string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize));

            return hashedInput == hash;
        }

        /// <param name="contraseña">La contraseña a verificar.</param>
        /// <returns>True si la contraseña es segura, false en caso contrario.</returns>
        public bool EsContraseñaSegura(string contraseña)
        {
            if (string.IsNullOrEmpty(contraseña)) return false;

            // Expresión regular para validar la seguridad de la contraseña
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
            return regex.IsMatch(contraseña);
        }

 
        /// <param name="usuario">El usuario al que se le asignarán los permisos.</param>
        public void AsignarPermisosParaUsuario(ApplicationDbContext context, Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (usuario.Rol?.Nombre != "Coordinador")
            {
                // Obtener los permisos necesarios de forma eficiente
                var permisos = context.Permisos
                    .Where(p => p.Nombre == "VerPerfilPropio" || p.Nombre == "ActualizarPerfilPropio")
                    .ToList();

                if (permisos.Any())
                {
                    foreach (var permiso in permisos)
                    {
                        if (!context.UsuarioPermisos.Any(up => up.UsuarioId == usuario.Id && up.PermisoId == permiso.Id))
                        {
                            context.UsuarioPermisos.Add(new UsuarioPermiso
                            {
                                UsuarioId = usuario.Id,
                                PermisoId = permiso.Id
                            });
                        }
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
