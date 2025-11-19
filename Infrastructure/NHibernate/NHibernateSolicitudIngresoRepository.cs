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

        public IEnumerable<SolicitudIngreso> ReadAll()
        {
            var q = _session.CreateQuery("from SolicitudIngreso");
            return q.List<SolicitudIngreso>();
        }

        public IEnumerable<SolicitudIngreso> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from SolicitudIngreso s where s.Solicitante is not null and lower(s.Solicitante.Nick) like :f");
            q.SetParameter("f", f);
            return q.List<SolicitudIngreso>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<SolicitudIngreso> BuscarSolicitudesIngresoPorNickSolicitante(string filtro) => ReadFilter(filtro);

        public void New(SolicitudIngreso entity) => _session.Save(entity);
        public void Modify(SolicitudIngreso entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
