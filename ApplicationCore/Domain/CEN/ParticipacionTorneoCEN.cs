using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ParticipacionTorneoCEN
    {
        private readonly IRepository<ParticipacionTorneo> _repo;

        public ParticipacionTorneoCEN(IRepository<ParticipacionTorneo> repo)
        {
            _repo = repo;
        }

        public ParticipacionTorneo NewParticipacionTorneo(Equipo equipo, Torneo torneo)
        {
            var p = new ParticipacionTorneo { Equipo = equipo, Torneo = torneo, Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.PENDIENTE.ToString(), FechaAlta = System.DateTime.UtcNow };
            _repo.New(p);
            return p;
        }

        public ParticipacionTorneo? ReadOID_ParticipacionTorneo(long id) => _repo.ReadById(id);
        public IEnumerable<ParticipacionTorneo> ReadAll_ParticipacionTorneo() => _repo.ReadAll();
        public void ModifyParticipacionTorneo(ParticipacionTorneo p) => _repo.Modify(p);
        public void DestroyParticipacionTorneo(long id) => _repo.Destroy(id);
        public IEnumerable<ParticipacionTorneo> ReadFilter_ParticipacionTorneo(string filter) => _repo.ReadFilter(filter);
    }
}
