using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernatePropuestaTorneoRepository : IRepository<PropuestaTorneo>
    {
        private readonly ISession _session;

        public NHibernatePropuestaTorneoRepository(ISession session)
        {
            _session = session;
        }

        public PropuestaTorneo? ReadById(long id) => _session.Get<PropuestaTorneo>(id);

        public IEnumerable<PropuestaTorneo> ReadAll()
        {
            var q = _session.CreateQuery("from PropuestaTorneo");
            return q.List<PropuestaTorneo>();
        }

        public IEnumerable<PropuestaTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from PropuestaTorneo p where p.Torneo is not null and lower(p.Torneo.Nombre) like :f");
            q.SetParameter("f", f);
            return q.List<PropuestaTorneo>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<PropuestaTorneo> BuscarPropuestasTorneoPorNombreTorneo(string filtro) => ReadFilter(filtro);

        public void New(PropuestaTorneo entity) => _session.Save(entity);
        public void Modify(PropuestaTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
