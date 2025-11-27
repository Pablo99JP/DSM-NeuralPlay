using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateReaccionRepository : IRepository<Reaccion>, IReaccionRepository
    {
        private readonly ISession _session;

        public NHibernateReaccionRepository(ISession session)
        {
            _session = session;
        }

        public Reaccion? ReadById(long id) => _session.Get<Reaccion>(id);

        public IEnumerable<Reaccion> ReadAll()
        {
            var q = _session.CreateQuery("from Reaccion");
            return q.List<Reaccion>();
        }

        public IEnumerable<Reaccion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Reaccion r where (r.Autor is not null and lower(r.Autor.Nick) like :f) or (r.Publicacion is not null and lower(r.Publicacion.Contenido) like :f)");
            q.SetParameter("f", f);
            return q.List<Reaccion>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Reaccion> BuscarReaccionesPorAutorNickOPublicacionContenido(string filtro) => ReadFilter(filtro);

        public void New(Reaccion entity) => _session.Save(entity);
        public void Modify(Reaccion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        // Efficient helpers
        public Reaccion? GetByPublicacionAndAutor(long publicacionId, long autorId)
        {
            return _session.Query<Reaccion>()
                .Where(r => r.Publicacion != null && r.Publicacion.IdPublicacion == publicacionId && r.Autor != null && r.Autor.IdUsuario == autorId && r.Tipo == ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA)
                .FirstOrDefault();
        }

        public int CountByPublicacion(long publicacionId)
        {
            return _session.Query<Reaccion>()
                .Count(r => r.Publicacion != null && r.Publicacion.IdPublicacion == publicacionId && r.Tipo == ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA);
        }

        public Reaccion? GetByComentarioAndAutor(long comentarioId, long autorId)
        {
            return _session.Query<Reaccion>()
                .Where(r => r.Comentario != null && r.Comentario.IdComentario == comentarioId && r.Autor != null && r.Autor.IdUsuario == autorId && r.Tipo == ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA)
                .FirstOrDefault();
        }

        public int CountByComentario(long comentarioId)
        {
            return _session.Query<Reaccion>()
                .Count(r => r.Comentario != null && r.Comentario.IdComentario == comentarioId && r.Tipo == ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA);
        }
    }
}
