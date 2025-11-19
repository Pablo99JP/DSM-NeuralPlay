using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    public class InMemoryMiembroEquipoRepository : InMemoryRepository<MiembroEquipo>, IMiembroEquipoRepository
    {
        public IEnumerable<Usuario> GetUsuariosByEquipo(long idEquipo)
        {
            return ReadAll()
                .Where(m => m.Equipo != null && m.Equipo.IdEquipo == idEquipo)
                .Select(m => m.Usuario)
                .Where(u => u != null)
                .Distinct()
                .ToList();
        }
    }
}
