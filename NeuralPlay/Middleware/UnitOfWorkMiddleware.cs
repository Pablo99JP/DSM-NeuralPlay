using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Middleware
{
    // Middleware simple que llama a SaveChanges() en el IUnitOfWork inyectado al final de la petición
    public class UnitOfWorkMiddleware
    {
        private readonly RequestDelegate _next;

        public UnitOfWorkMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUnitOfWork? uow)
        {
            try
            {
                await _next(context);
                // Intentar persistir cambios (si hay unidad de trabajo activa)
                try { uow?.SaveChanges(); } catch { /* swallow to avoid hiding original response errors */ }
            }
            finally
            {
                // If uow implements IDisposable, DI container will dispose it at scope end; nothing to do here.
            }
        }
    }
}
