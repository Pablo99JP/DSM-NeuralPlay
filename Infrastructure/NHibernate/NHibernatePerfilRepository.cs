using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernatePerfilRepository : IRepository<Perfil>
    {
        private readonly ISession _session;

        public NHibernatePerfilRepository(ISession session)
        {
            _session = session;
        }

        public Perfil? ReadById(long id) => _session.Get<Perfil>(id);

        public IEnumerable<Perfil> ReadAll() => _session.Query<Perfil>().ToList();

        public IEnumerable<Perfil> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Perfil>().Where(p => p.Descripcion != null && p.Descripcion.ToLower().Contains(f)).ToList();
        }

        public void New(Perfil entity) => _session.Save(entity);
        public void Modify(Perfil entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
