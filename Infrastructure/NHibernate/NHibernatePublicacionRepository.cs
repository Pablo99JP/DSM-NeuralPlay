using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernatePublicacionRepository : IRepository<Publicacion>
    {
        private readonly ISession _session;

        public NHibernatePublicacionRepository(ISession session)
        {
            _session = session;
        }

        public Publicacion? ReadById(long id) => _session.Get<Publicacion>(id);

        public IEnumerable<Publicacion> ReadAll() => _session.Query<Publicacion>().ToList();

        public IEnumerable<Publicacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Publicacion>().Where(p => p.Contenido.ToLower().Contains(f)).ToList();
        }

        public void New(Publicacion entity) => _session.Save(entity);
        public void Modify(Publicacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
