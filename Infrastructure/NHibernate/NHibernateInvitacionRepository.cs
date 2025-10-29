using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateInvitacionRepository : IRepository<Invitacion>
    {
        private readonly ISession _session;

        public NHibernateInvitacionRepository(ISession session)
        {
            _session = session;
        }

        public Invitacion? ReadById(long id) => _session.Get<Invitacion>(id);

        public IEnumerable<Invitacion> ReadAll() => _session.Query<Invitacion>().ToList();

        public IEnumerable<Invitacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Invitacion>().Where(i => (i.Emisor != null && i.Emisor.Nick.ToLower().Contains(f)) || (i.Destinatario != null && i.Destinatario.Nick.ToLower().Contains(f))).ToList();
        }

        public void New(Invitacion entity) => _session.Save(entity);
        public void Modify(Invitacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
