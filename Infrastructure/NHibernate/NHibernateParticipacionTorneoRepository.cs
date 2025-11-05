using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateParticipacionTorneoRepository : IParticipacionTorneoRepository
    {
        private readonly ISession _session;

        public NHibernateParticipacionTorneoRepository(ISession session)
        {
            _session = session;
        }

        public ParticipacionTorneo? ReadById(long id) => _session.Get<ParticipacionTorneo>(id);

        public IEnumerable<ParticipacionTorneo> ReadAll() => _session.Query<ParticipacionTorneo>().ToList();

        public IEnumerable<ParticipacionTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<ParticipacionTorneo>().Where(p => p.Estado.ToLower().Contains(f)).ToList();
        }

        public void New(ParticipacionTorneo entity) => _session.Save(entity);
        public void Modify(ParticipacionTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public IEnumerable<Equipo> GetEquiposByTorneo(long idTorneo)
        {
            return _session.Query<ParticipacionTorneo>()
                .Where(p => p.Torneo != null && p.Torneo.IdTorneo == idTorneo && p.Equipo != null)
                .Select(p => p.Equipo)
                .Distinct()
                .ToList();
        }

        public IEnumerable<Torneo> GetTorneosByEquipo(long idEquipo)
        {
            return _session.Query<ParticipacionTorneo>()
                .Where(p => p.Equipo != null && p.Equipo.IdEquipo == idEquipo && p.Torneo != null)
                .Select(p => p.Torneo)
                .Distinct()
                .ToList();
        }
    }
}
