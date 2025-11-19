using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    public class InMemoryMiembroComunidadRepository : InMemoryRepository<MiembroComunidad>, IMiembroComunidadRepository
    {
        public IEnumerable<Usuario> GetUsuariosByComunidad(long idComunidad)
        {
            return ReadAll()
                .Where(m => m.Comunidad != null && m.Comunidad.IdComunidad == idComunidad)
                .Select(m => m.Usuario)
                .Where(u => u != null)
                .Distinct()
                .ToList();
        }
    }
}
