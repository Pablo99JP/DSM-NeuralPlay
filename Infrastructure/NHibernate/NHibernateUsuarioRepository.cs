using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateUsuarioRepository : IUsuarioRepository
    {
        private readonly ISession _session;

        public NHibernateUsuarioRepository(ISession session)
        {
            _session = session;
        }

        public Usuario? ReadById(long id)
        {
            return _session.Get<Usuario>(id);
        }

        public IEnumerable<Usuario> ReadAll()
        {
            var q = _session.CreateQuery("from Usuario");
            return q.List<Usuario>();
        }

        public void New(Usuario entity)
        {
            _session.Save(entity);
        }

        public void Modify(Usuario entity)
        {
            _session.Update(entity);
        }

        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        public Usuario? ReadByNick(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return null;
            var q = _session.CreateQuery("from Usuario u where lower(u.Nick) = :nick");
            q.SetParameter("nick", nick.ToLower());
            return q.UniqueResult<Usuario>();
        }

        public Usuario? ReadByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var q = _session.CreateQuery("from Usuario u where lower(u.CorreoElectronico) = :email");
            q.SetParameter("email", email.ToLower());
            return q.UniqueResult<Usuario>();
        }

        public IEnumerable<Usuario> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Usuario u where (lower(u.Nick) like :f) or (lower(u.CorreoElectronico) like :f)");
            q.SetParameter("f", f);
            return q.List<Usuario>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Usuario> BuscarUsuariosPorNickOEmail(string filtro) => ReadFilter(filtro);
    }
}
