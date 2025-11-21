using ApplicationCore.Domain.EN;

namespace NeuralPlay.Services
{
    /// <summary>
    /// Servicio para obtener el usuario autenticado a partir de la sesión HTTP.
    /// </summary>
    public interface IUsuarioAuth
    {
        /// <summary>
        /// Devuelve el usuario actual o null si no hay sesión.
        /// </summary>
        Usuario? GetUsuarioActual();
    }
}
