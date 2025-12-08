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

        public IEnumerable<Comentario> ReadAll()
        {
            var q = _session.CreateQuery("from Comentario");
            return q.List<Comentario>();
        }

        public IEnumerable<Comentario> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Comentario c where lower(c.Contenido) like :f");
            q.SetParameter("f", f);
            return q.List<Comentario>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Comentario> BuscarComentariosPorContenido(string filtro) => ReadFilter(filtro);

        /// <summary>
        /// Obtiene todos los comentarios creados por un usuario específico, ordenados por fecha descendente
        /// </summary>
        public IEnumerable<Comentario> GetComentariosPorAutor(long autorId)
        {
            var q = _session.CreateQuery("from Comentario c where c.Autor.IdUsuario = :autorId order by c.FechaCreacion desc");
            q.SetParameter("autorId", autorId);
            return q.List<Comentario>();
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
