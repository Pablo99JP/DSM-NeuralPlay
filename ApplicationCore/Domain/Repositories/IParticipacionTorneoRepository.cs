using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IParticipacionTorneoRepository : IRepository<ParticipacionTorneo>
    {
        IEnumerable<Equipo> GetEquiposByTorneo(long idTorneo);
        IEnumerable<Torneo> GetTorneosByEquipo(long idEquipo);
    }
}
