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

        // Ensure collections are initialized to avoid lazy-loading issues in views
        public Publicacion? ReadById(long id)
        {
            try
            {
                var en = _session.Get<Publicacion>(id);
                if (en == null) return null;

                // Initialize collections and related entities
                try
                {
                    NHibernateUtil.Initialize(en.Comentarios);
                    NHibernateUtil.Initialize(en.Reacciones);

                    // Ensure comment authors and comment reactions are initialized
                    foreach (var c in en.Comentarios ?? System.Array.Empty<Comentario>())
                    {
                        NHibernateUtil.Initialize(c.Autor);
                        NHibernateUtil.Initialize(c.Reacciones);
                    }
                }
                catch
                {
                    // Ignore initialization errors; fall back to returning the entity as-is
                }

                return en;
            }
            catch
            {
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
