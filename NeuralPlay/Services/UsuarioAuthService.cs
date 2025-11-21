using Microsoft.AspNetCore.Http;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.EN;

namespace NeuralPlay.Services
{
    /// <summary>
    /// Implementación simple de IUsuarioAuth que lee el id de sesión y recupera la entidad desde el repositorio.
    /// </summary>
    public class UsuarioAuthService : IUsuarioAuth
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioAuthService(IHttpContextAccessor accessor, IUsuarioRepository usuarioRepository)
        {
            _accessor = accessor;
            _usuarioRepository = usuarioRepository;
        }

        public Usuario? GetUsuarioActual()
        {
            var ctx = _accessor.HttpContext;
            if (ctx == null) return null;

            var id = ctx.Session.GetInt32("UsuarioId");
            if (!id.HasValue) return null;

            return _usuarioRepository.ReadById(id.Value);
        }
    }
}
