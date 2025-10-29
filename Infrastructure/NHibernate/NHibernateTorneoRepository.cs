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

        public IEnumerable<Torneo> ReadAll() => _session.Query<Torneo>().ToList();

        public IEnumerable<Torneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Torneo>().Where(t => t.Nombre.ToLower().Contains(f) || (t.Estado != null && t.Estado.ToLower().Contains(f))).ToList();
        }

        public void New(Torneo entity) => _session.Save(entity);
        public void Modify(Torneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
