using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateSolicitudIngresoRepository : IRepository<SolicitudIngreso>
    {
        private readonly ISession _session;

        public NHibernateSolicitudIngresoRepository(ISession session)
        {
            _session = session;
        }

        public SolicitudIngreso? ReadById(long id) => _session.Get<SolicitudIngreso>(id);

        public IEnumerable<SolicitudIngreso> ReadAll() => _session.Query<SolicitudIngreso>().ToList();

        public IEnumerable<SolicitudIngreso> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<SolicitudIngreso>().Where(s => (s.Solicitante != null && s.Solicitante.Nick.ToLower().Contains(f))).ToList();
        }

        public void New(SolicitudIngreso entity) => _session.Save(entity);
        public void Modify(SolicitudIngreso entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
