using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Vanessa.Interfaces;

namespace Vanessa.Services
{
    public class AuthService : IAuthService
    {
        // Sin dependencia de ApplicationDbContext — solo lógica de hash/validación
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int IterationCount = 10000;

        public string ConvertirContraseña(string contraseña)
        {
            if (string.IsNullOrWhiteSpace(contraseña))
                throw new ArgumentException("La contraseña no puede estar vacía.", nameof(contraseña));

            byte[] salt = new byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize));

            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        public bool VerificarContraseña(string hashCompleto, string contraseña)
        {
            if (string.IsNullOrWhiteSpace(hashCompleto) || string.IsNullOrWhiteSpace(contraseña))
                return false;

            var parts = hashCompleto.Split('.');
            if (parts.Length != 2) return false;

            byte[] salt;
            try { salt = Convert.FromBase64String(parts[0]); }
            catch { return false; }

            string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: contraseña,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: IterationCount,
                numBytesRequested: HashSize));

            return hashedInput == parts[1];
        }

        public bool EsContraseñaSegura(string contraseña)
        {
            if (string.IsNullOrEmpty(contraseña)) return false;
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");
            return regex.IsMatch(contraseña);
        }
    }
}
