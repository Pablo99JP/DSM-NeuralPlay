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

        public IEnumerable<ParticipacionTorneo> ReadAll()
        {
            var q = _session.CreateQuery("from ParticipacionTorneo");
            return q.List<ParticipacionTorneo>();
        }

        public IEnumerable<ParticipacionTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from ParticipacionTorneo p where lower(p.Estado) like :f");
            q.SetParameter("f", f);
            return q.List<ParticipacionTorneo>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<ParticipacionTorneo> BuscarParticipacionesTorneoPorEstado(string filtro) => ReadFilter(filtro);

        public void New(ParticipacionTorneo entity) => _session.Save(entity);
        public void Modify(ParticipacionTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public IEnumerable<Equipo> GetEquiposByTorneo(long idTorneo)
        {
            var q = _session.CreateQuery("select distinct p.Equipo from ParticipacionTorneo p where p.Torneo is not null and p.Torneo.IdTorneo = :idTorneo and p.Equipo is not null");
            q.SetParameter("idTorneo", idTorneo);
            return q.List<Equipo>();
        }

        public IEnumerable<Torneo> GetTorneosByEquipo(long idEquipo)
        {
            var q2 = _session.CreateQuery("select distinct p.Torneo from ParticipacionTorneo p where p.Equipo is not null and p.Equipo.IdEquipo = :idEquipo and p.Torneo is not null");
            q2.SetParameter("idEquipo", idEquipo);
            return q2.List<Torneo>();
        }
    }
}
