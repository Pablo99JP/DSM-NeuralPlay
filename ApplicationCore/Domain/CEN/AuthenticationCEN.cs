using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    // PBKDF2 password hasher. Produces hashes in the format: {iterations}.{saltBase64}.{hashBase64}
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100_000; // reasonable default; adjust based on hardware

        public static string Hash(string plain)
        {
            if (plain == null) throw new ArgumentNullException(nameof(plain));

            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            var parts = new[] { Iterations.ToString(), Convert.ToBase64String(salt), Convert.ToBase64String(hash) };
            return string.Join(".", parts);
        }

        public static bool Verify(string plain, string storedHash)
        {
            if (plain == null) throw new ArgumentNullException(nameof(plain));
            if (string.IsNullOrWhiteSpace(storedHash)) return false;

            var parts = storedHash.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iterations)) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(plain, salt, iterations, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(hash.Length);

            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }

    public class AuthenticationCEN
    {
        private readonly IUsuarioRepository _usuarioRepo;

        public AuthenticationCEN(IUsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        public Usuario? Login(string nickOrEmail, string password)
        {
            var byNick = _usuarioRepo.ReadByNick(nickOrEmail);
            Usuario? user = byNick ?? _usuarioRepo.ReadFilter(nickOrEmail).FirstOrDefault(u => u.CorreoElectronico == nickOrEmail);
            if (user == null) return null;
            if (PasswordHasher.Verify(password, user.ContrasenaHash)) return user;
            return null;
        }
    }
}
