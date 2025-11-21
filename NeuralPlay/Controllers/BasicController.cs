using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    /// <summary>
    /// Controlador base simple para centralizar servicios compartidos.
    /// Simula la inyección de UsuarioCEN e IUsuarioRepository.
    /// </summary>
    public abstract class BasicController : Controller
    {
        protected readonly UsuarioCEN _usuarioCEN;
        protected readonly IUsuarioRepository _usuarioRepository;

        protected BasicController(UsuarioCEN usuarioCEN, IUsuarioRepository usuarioRepository)
        {
            _usuarioCEN = usuarioCEN;
            _usuarioRepository = usuarioRepository;
        }
    }
}
