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

        public Comunidad? ReadById(long id) => _session.Get<Comunidad>(id);

        public IEnumerable<Comunidad> ReadAll() => _session.Query<Comunidad>().ToList();

        public IEnumerable<Comunidad> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Comunidad>().Where(c => c.Nombre.ToLower().Contains(f) || (c.Descripcion != null && c.Descripcion.ToLower().Contains(f))).ToList();
        }

        public void New(Comunidad entity) => _session.Save(entity);
        public void Modify(Comunidad entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
