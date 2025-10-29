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

        public IEnumerable<Notificacion> ReadAll() => _session.Query<Notificacion>().ToList();

        public IEnumerable<Notificacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Notificacion>().Where(n => n.Mensaje.ToLower().Contains(f) || (n.Destinatario != null && n.Destinatario.Nick.ToLower().Contains(f))).ToList();
        }

        public void New(Notificacion entity) => _session.Save(entity);
        public void Modify(Notificacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
