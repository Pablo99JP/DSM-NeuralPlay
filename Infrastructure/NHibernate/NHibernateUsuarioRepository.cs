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
            return _session.Query<Usuario>().ToList();
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
            return _session.Query<Usuario>().FirstOrDefault(u => u.Nick.ToLower() == nick.ToLower());
        }

        public IEnumerable<Usuario> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Usuario>().Where(u => (u.Nick != null && u.Nick.ToLower().Contains(f)) || (u.CorreoElectronico != null && u.CorreoElectronico.ToLower().Contains(f))).ToList();
        }
    }
}
