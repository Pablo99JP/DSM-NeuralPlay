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

        public IEnumerable<VotoTorneo> ReadAll()
        {
            var q = _session.CreateQuery("from VotoTorneo");
            return q.List<VotoTorneo>();
        }

        public IEnumerable<VotoTorneo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from VotoTorneo v where v.Propuesta is not null and v.Propuesta.Torneo is not null and lower(v.Propuesta.Torneo.Nombre) like :f");
            q.SetParameter("f", f);
            return q.List<VotoTorneo>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<VotoTorneo> BuscarVotosTorneoPorNombreTorneo(string filtro) => ReadFilter(filtro);

        public void New(VotoTorneo entity) => _session.Save(entity);
        public void Modify(VotoTorneo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
