using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NeuralPlay.Middleware
{
    /// <summary>
    /// Middleware de autenticación que protege las rutas excepto login y registro.
    /// Redirige a login si el usuario intenta acceder sin estar autenticado.
    /// </summary>
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        // Rutas públicas (no requieren autenticación)
        private static readonly HashSet<string> PublicRoutes = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Usuario/Login",
            "/Usuario/Create",
            "/Usuario/LogoutConfirm",
            "/Home/Privacy",
            "/Home/Error",
            "/css",
            "/js",
            "/lib",
            "/images",
            "/Recursos"
        };

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "/";

            // Verificar si la ruta es pública
            bool isPublicRoute = IsPublicRoute(path);

            // Si no es una ruta pública y el usuario no está autenticado, redirigir a login
            if (!isPublicRoute && !IsUserAuthenticated(context))
            {
                context.Response.Redirect("/Usuario/Login");
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Verifica si una ruta es pública (no requiere autenticación)
        /// </summary>
        private bool IsPublicRoute(string path)
        {
            return PublicRoutes.Any(route => path.StartsWith(route, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Verifica si el usuario está autenticado mediante sesión
        /// </summary>
        private bool IsUserAuthenticated(HttpContext context)
        {
            // Verificar si existe UsuarioId en la sesión
            return context.Session.GetInt32("UsuarioId").HasValue && 
                   context.Session.GetInt32("UsuarioId") > 0;
        }
    }
}
