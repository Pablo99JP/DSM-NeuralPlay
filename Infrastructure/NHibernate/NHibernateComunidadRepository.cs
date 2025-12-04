using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateComunidadRepository : IRepository<Comunidad>
    {
        private readonly ISession _session;

        public NHibernateComunidadRepository(ISession session)
        {
            _session = session;
        }

        public Comunidad? ReadById(long id)
        {
            var comunidad = _session.Get<Comunidad>(id);
            if (comunidad != null)
            {
                // Inicializar colecciones para evitar lazy loading
                NHibernateUtil.Initialize(comunidad.Miembros);
                NHibernateUtil.Initialize(comunidad.Publicaciones);
                NHibernateUtil.Initialize(comunidad.Torneos);
                NHibernateUtil.Initialize(comunidad.Equipos);
                
                // Inicializar el autor de cada publicación
                foreach (var pub in comunidad.Publicaciones ?? System.Array.Empty<Publicacion>())
                {
                    NHibernateUtil.Initialize(pub.Autor);
                }
            }
            return comunidad;
        }

        public IEnumerable<Comunidad> ReadAll()
        {
            var q = _session.CreateQuery("from Comunidad");
            return q.List<Comunidad>();
        }

        public IEnumerable<Comunidad> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Comunidad c where lower(c.Nombre) like :f or (c.Descripcion is not null and lower(c.Descripcion) like :f)");
            q.SetParameter("f", f);
            return q.List<Comunidad>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Comunidad> BuscarComunidadesPorNombreODescripcion(string filtro) => ReadFilter(filtro);

        public void New(Comunidad entity) => _session.Save(entity);
        public void Modify(Comunidad entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
