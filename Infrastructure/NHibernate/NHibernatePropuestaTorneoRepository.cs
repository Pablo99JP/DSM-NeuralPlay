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

        public IEnumerable<PropuestaTorneo> ReadAll() => _session.Query<PropuestaTorneo>().ToList();

        public IEnumerable<PropuestaTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<PropuestaTorneo>().Where(p => p.Torneo != null && p.Torneo.Nombre.ToLower().Contains(f)).ToList();
        }

        public void New(PropuestaTorneo entity) => _session.Save(entity);
        public void Modify(PropuestaTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
