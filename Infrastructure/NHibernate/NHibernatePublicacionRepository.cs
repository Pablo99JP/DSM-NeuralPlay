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

        // Use LINQ with FetchMany to ensure Comentarios (and their Autor) are loaded eagerly
        public Publicacion? ReadById(long id)
        {
            try
            {
                var q = _session.Query<Publicacion>()
                    .FetchMany(p => p.Comentarios)
                    .ThenFetch(c => c.Autor)
                    .Where(p => p.IdPublicacion == id)
                    .ToFutureValue();

                // ToFutureValue returns an IFutureValue<Publicacion>; Value triggers execution
                return q.Value;
            }
            catch
            {
                // Fallback to simple Get if LINQ fetch fails for any reason
                return _session.Get<Publicacion>(id);
            }
        }

        public IEnumerable<Publicacion> ReadAll()
        {
            var q = _session.CreateQuery("from Publicacion");
            return q.List<Publicacion>();
        }

        public IEnumerable<Publicacion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Publicacion p where lower(p.Contenido) like :f");
            q.SetParameter("f", f);
            return q.List<Publicacion>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Publicacion> BuscarPublicacionesPorContenido(string filtro) => ReadFilter(filtro);

        public void New(Publicacion entity) => _session.Save(entity);
        public void Modify(Publicacion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
