using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public static class PasswordHasher
    {
        public static string Hash(string plain)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(plain);
            var hash = sha.ComputeHash(bytes);
            return System.Convert.ToHexString(hash);
        }

        public static bool Verify(string plain, string hash) => Hash(plain) == hash;
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
