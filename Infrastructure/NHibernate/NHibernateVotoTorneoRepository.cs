using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateVotoTorneoRepository : IRepository<VotoTorneo>
    {
        private readonly ISession _session;

        public NHibernateVotoTorneoRepository(ISession session)
        {
            _session = session;
        }

        public VotoTorneo? ReadById(long id) => _session.Get<VotoTorneo>(id);

        public IEnumerable<VotoTorneo> ReadAll() => _session.Query<VotoTorneo>().ToList();

        public IEnumerable<VotoTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<VotoTorneo>().Where(v => v.Propuesta != null && v.Propuesta.Torneo != null && v.Propuesta.Torneo.Nombre.ToLower().Contains(f)).ToList();
        }

        public void New(VotoTorneo entity) => _session.Save(entity);
        public void Modify(VotoTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
