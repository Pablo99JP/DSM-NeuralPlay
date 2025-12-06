using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationCore.Domain.CEN
{
    // Componente sin estado que agrupa operaciones CRUD para Usuario
    public class UsuarioCEN
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioCEN(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public Usuario NewUsuario(string nick, string correo, string password)
        {
            var u = new Usuario
            {
                Nick = nick,
                CorreoElectronico = correo,
                ContrasenaHash = PasswordHasher.Hash(password),
                FechaRegistro = System.DateTime.UtcNow,
                EstadoCuenta = ApplicationCore.Domain.Enums.EstadoCuenta.ACTIVA
            };

            // --- INICIO DE LA CORRECCIÓN ---
            // 1. Crear el perfil asociado
            var perfil = new Perfil
            {
                Usuario = u, // Establecer la relación bidireccional
                Descripcion = $"Perfil de {nick}.",
                FotoPerfilUrl = "/Recursos/Perfiles/Default.png", // Asignar una imagen por defecto
                VisibilidadPerfil = Visibilidad.PUBLICO,
                VisibilidadActividad = Visibilidad.PRIVADO
            };

            // 2. Asignar el perfil al usuario
            u.Perfil = perfil;
            // --- FIN DE LA CORRECCIÓN ---

            _usuarioRepository.New(u);
            // Retornar directamente el usuario creado
            return u;
        }

        public Usuario? ReadOID_Usuario(long id) => _usuarioRepository.ReadById(id);
        public System.Collections.Generic.IEnumerable<Usuario> ReadAll_Usuario() => _usuarioRepository.ReadAll();
        
        // Login: busca por email y verifica la contraseña con PBKDF2
        public Usuario? Login(string email, string password)
        {
            var u = _usuarioRepository.ReadByEmail(email);
            if (u == null) return null;
            return PasswordHasher.Verify(password, u.ContrasenaHash) ? u : null;
        }

        // Detecta si el valor proporcionado ya tiene el formato de hash PBKDF2 usado (iterations.salt.hash)
        private static bool LooksLikeHashed(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var parts = value.Split('.');
            if (parts.Length != 3) return false;
            return int.TryParse(parts[0], out _);
        }

        
        public void ModifyUsuario(Usuario usuario)
        {
            if (usuario == null) throw new System.ArgumentNullException(nameof(usuario));

            // Si la propiedad ContrasenaHash contiene una contraseña en claro (p. ej. provista desde el formulario),
            // hashearla antes de enviar al repositorio. Si ya tiene el formato de hash PBKDF2, no hacemos nada.
            if (!string.IsNullOrWhiteSpace(usuario.ContrasenaHash) && !LooksLikeHashed(usuario.ContrasenaHash))
            {
                usuario.ContrasenaHash = PasswordHasher.Hash(usuario.ContrasenaHash);
            }

            _usuarioRepository.Modify(usuario);
        }
        public void DestroyUsuario(long id) => _usuarioRepository.Destroy(id);
        public System.Collections.Generic.IEnumerable<Usuario> BuscarUsuariosPorNickOEmail(string filtro) => _usuarioRepository.ReadFilter(filtro);

        // Verificar si ya existe un usuario con el mismo email
        public bool ExisteEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return _usuarioRepository.ReadByEmail(email) != null;
        }

        // Verificar si ya existe un usuario con el mismo nick
        public bool ExisteNick(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return false;
            return _usuarioRepository.ReadByNick(nick) != null;
        }
    }
}
