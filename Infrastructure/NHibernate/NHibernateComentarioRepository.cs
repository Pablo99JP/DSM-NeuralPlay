using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateComentarioRepository : IRepository<Comentario>
    {
        private readonly ISession _session;

        public NHibernateComentarioRepository(ISession session)
        {
            _session = session;
        }

        public Comentario? ReadById(long id) => _session.Get<Comentario>(id);

        public IEnumerable<Comentario> ReadAll() => _session.Query<Comentario>().ToList();

        public IEnumerable<Comentario> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Comentario>().Where(c => c.Contenido.ToLower().Contains(f)).ToList();
        }

        public void New(Comentario entity) => _session.Save(entity);
        public void Modify(Comentario entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
