using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateTorneoRepository : IRepository<Torneo>
    {
        private readonly ISession _session;

        public NHibernateTorneoRepository(ISession session)
        {
            _session = session;
        }

        public Torneo? ReadById(long id) => _session.Get<Torneo>(id);

        public IEnumerable<Torneo> ReadAll()
        {
            var q = _session.CreateQuery("from Torneo");
            return q.List<Torneo>();
        }

        public IEnumerable<Torneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Torneo t where lower(t.Nombre) like :f or (t.Estado is not null and lower(t.Estado) like :f)");
            q.SetParameter("f", f);
            return q.List<Torneo>();
        }

            // Descriptive wrapper used by CENs or higher layers
            public IEnumerable<Torneo> BuscarTorneosPorNombre(string filtro) => ReadFilter(filtro);

        public void New(Torneo entity) => _session.Save(entity);
        public void Modify(Torneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
