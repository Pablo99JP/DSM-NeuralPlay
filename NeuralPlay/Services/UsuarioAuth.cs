using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Http;
using NHibernate.Linq; // Añadir este using para Fetch
using System.Linq;
using System.Threading.Tasks;

namespace NeuralPlay.Services
{
    public class UsuarioAuth : IUsuarioAuth
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioAuth(IHttpContextAccessor httpContextAccessor, IUsuarioRepository usuarioRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _usuarioRepository = usuarioRepository;
        }

        public Usuario? GetUsuarioActual()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UsuarioId");
            if (userId == null)
            {
                return null;
            }

            // --- INICIO DE LA CORRECCIÓN ---
            // Usamos Fetch para cargar explícitamente el Perfil junto con el Usuario
            using (var session = NHibernateHelper.OpenSession())
            {
                var usuario = session.Query<Usuario>()
                                     .Fetch(u => u.Perfil) // Carga ansiosa (eager loading) del perfil
                                     .FirstOrDefault(u => u.IdUsuario == userId.Value);
                return usuario;
            }
            // --- FIN DE LA CORRECCIÓN ---
        }

        public async Task Login(Usuario usuario)
        {
            _httpContextAccessor.HttpContext?.Session.SetInt32("UsuarioId", (int)usuario.IdUsuario);
            await Task.CompletedTask;
        }

        public async Task Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("UsuarioId");
            await Task.CompletedTask;
        }
    }
}