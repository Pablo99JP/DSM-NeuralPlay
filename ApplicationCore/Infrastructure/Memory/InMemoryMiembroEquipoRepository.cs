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

        public IEnumerable<Equipo> GetEquiposByUsuario(long idUsuario)
        {
            return ReadAll()
                .Where(m => m.Usuario != null && m.Usuario.IdUsuario == idUsuario && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                .Select(m => m.Equipo)
                .Where(e => e != null)
                .Distinct()
                .ToList();
        }
    }
}
