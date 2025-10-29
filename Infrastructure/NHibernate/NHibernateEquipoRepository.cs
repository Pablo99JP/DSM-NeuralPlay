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

        public IEnumerable<Equipo> ReadAll() => _session.Query<Equipo>().ToList();

        public IEnumerable<Equipo> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Equipo>().Where(e => e.Nombre.ToLower().Contains(f) || (e.Descripcion != null && e.Descripcion.ToLower().Contains(f))).ToList();
        }

        public void New(Equipo entity) => _session.Save(entity);
        public void Modify(Equipo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
