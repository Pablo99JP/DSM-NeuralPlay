using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateReaccionRepository : IRepository<Reaccion>
    {
        private readonly ISession _session;

        public NHibernateReaccionRepository(ISession session)
        {
            _session = session;
        }

        public Reaccion? ReadById(long id) => _session.Get<Reaccion>(id);

        public IEnumerable<Reaccion> ReadAll() => _session.Query<Reaccion>().ToList();

        public IEnumerable<Reaccion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Reaccion>().Where(r => (r.Autor != null && r.Autor.Nick.ToLower().Contains(f)) || (r.Publicacion != null && r.Publicacion.Contenido.ToLower().Contains(f))).ToList();
        }

        public void New(Reaccion entity) => _session.Save(entity);
        public void Modify(Reaccion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
