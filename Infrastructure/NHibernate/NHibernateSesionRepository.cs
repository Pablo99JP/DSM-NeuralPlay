using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateSesionRepository : IRepository<Sesion>
    {
        private readonly ISession _session;

        public NHibernateSesionRepository(ISession session)
        {
            _session = session;
        }

        public Sesion? ReadById(long id) => _session.Get<Sesion>(id);

        public IEnumerable<Sesion> ReadAll() => _session.Query<Sesion>().ToList();

        public IEnumerable<Sesion> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<Sesion>().Where(s => s.Token.ToLower().Contains(f)).ToList();
        }

        public void New(Sesion entity) => _session.Save(entity);
        public void Modify(Sesion entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
