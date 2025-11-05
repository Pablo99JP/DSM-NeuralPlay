using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    public class InMemoryParticipacionTorneoRepository : InMemoryRepository<ParticipacionTorneo>, IParticipacionTorneoRepository
    {
        public IEnumerable<Equipo> GetEquiposByTorneo(long idTorneo)
        {
            return ReadAll()
                .Where(p => p.Torneo != null && p.Torneo.IdTorneo == idTorneo)
                .Select(p => p.Equipo)
                .Where(e => e != null)
                .Distinct()
                .ToList();
        }

        public IEnumerable<Torneo> GetTorneosByEquipo(long idEquipo)
        {
            return ReadAll()
                .Where(p => p.Equipo != null && p.Equipo.IdEquipo == idEquipo)
                .Select(p => p.Torneo)
                .Where(t => t != null)
                .Distinct()
                .ToList();
        }
    }
}
