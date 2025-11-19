using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateEquipoRepository : IRepository<Equipo>
    {
        private readonly ISession _session;

        public NHibernateEquipoRepository(ISession session)
        {
            _session = session;
        }

        public Equipo? ReadById(long id) => _session.Get<Equipo>(id);

        public IEnumerable<Equipo> ReadAll()
        {
            var q = _session.CreateQuery("from Equipo");
            return q.List<Equipo>();
        }

        public IEnumerable<Equipo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Equipo e where lower(e.Nombre) like :f or (e.Descripcion is not null and lower(e.Descripcion) like :f)");
            q.SetParameter("f", f);
            return q.List<Equipo>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Equipo> BuscarEquiposPorNombreODescripcion(string filtro) => ReadFilter(filtro);

        public void New(Equipo entity) => _session.Save(entity);
        public void Modify(Equipo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
