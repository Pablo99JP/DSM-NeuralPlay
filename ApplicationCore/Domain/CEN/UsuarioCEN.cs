using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    // Stateless component that groups CRUD operations for Usuario
    public class UsuarioCEN
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioCEN(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Usuario NewUsuario(string nick, string correo, string contrasenaHash)
        {
            var u = new Usuario
            {
                Nick = nick,
                CorreoElectronico = correo,
                ContrasenaHash = contrasenaHash,
                FechaRegistro = System.DateTime.UtcNow,
                EstadoCuenta = ApplicationCore.Domain.Enums.EstadoCuenta.ACTIVA
            };
            _usuarioRepository.New(u);
            return u;
        }

        public Usuario? ReadOID_Usuario(long id) => _usuarioRepository.ReadById(id);
        public System.Collections.Generic.IEnumerable<Usuario> ReadAll_Usuario() => _usuarioRepository.ReadAll();
        
        // Login: busca por email y compara hash/contraseña (simplificado para pruebas)
        public Usuario? Login(string email, string password)
        {
            var u = _usuarioRepository.ReadByEmail(email);
            if (u == null) return null;
            // En un entorno real la contraseña estaría hasheada; aquí comparamos directamente
            return u.ContrasenaHash == password ? u : null;
        }
        public void ModifyUsuario(Usuario usuario) => _usuarioRepository.Modify(usuario);
        public void DestroyUsuario(long id) => _usuarioRepository.Destroy(id);
        public System.Collections.Generic.IEnumerable<Usuario> BuscarUsuariosPorNickOEmail(string filtro) => _usuarioRepository.ReadFilter(filtro);
    }
}
