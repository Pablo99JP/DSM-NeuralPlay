using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateNotificacionRepository : IRepository<Notificacion>
    {
        private readonly ISession _session;

        public NHibernateNotificacionRepository(ISession session)
        {
            _session = session;
        }

        public Notificacion? ReadById(long id) => _session.Get<Notificacion>(id);

        public IEnumerable<Notificacion> ReadAll()
        {
            var q = _session.CreateQuery("from Notificacion");
            return q.List<Notificacion>();
        }

        public IEnumerable<Notificacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Notificacion n where (lower(n.Mensaje) like :f) or (n.Destinatario is not null and lower(n.Destinatario.Nick) like :f)");
            q.SetParameter("f", f);
            return q.List<Notificacion>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Notificacion> BuscarNotificacionesPorMensajeODestinatarioNick(string filtro) => ReadFilter(filtro);

        public void New(Notificacion entity) => _session.Save(entity);
        public void Modify(Notificacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
