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

        public Invitacion? ReadById(long id)
        {
            var q = _session.CreateQuery("from Invitacion i left join fetch i.Emisor left join fetch i.Destinatario left join fetch i.Equipo left join fetch i.Comunidad where i.IdInvitacion = :id");
            q.SetParameter("id", id);
            return q.UniqueResult<Invitacion>();
        }

        public IEnumerable<Invitacion> ReadAll()
        {
            var q = _session.CreateQuery("from Invitacion");
            return q.List<Invitacion>();
        }

        public IEnumerable<Invitacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Invitacion i where (i.Emisor is not null and lower(i.Emisor.Nick) like :f) or (i.Destinatario is not null and lower(i.Destinatario.Nick) like :f)");
            q.SetParameter("f", f);
            return q.List<Invitacion>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Invitacion> BuscarInvitacionesPorNickEmisorODestinatario(string filtro) => ReadFilter(filtro);

        public void New(Invitacion entity) => _session.Save(entity);
        public void Modify(Invitacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
