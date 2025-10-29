using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernatePerfilJuegoRepository : IRepository<PerfilJuego>
    {
        private readonly ISession _session;

        public NHibernatePerfilJuegoRepository(ISession session)
        {
            _session = session;
        }

        public PerfilJuego? ReadById(long id) => _session.Get<PerfilJuego>(id);

        public IEnumerable<PerfilJuego> ReadAll() => _session.Query<PerfilJuego>().ToList();

        public IEnumerable<PerfilJuego> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = filter.ToLowerInvariant();
            return _session.Query<PerfilJuego>().Where(pj => pj.Juego != null && pj.Juego.NombreJuego.ToLower().Contains(f)).ToList();
        }

        public void New(PerfilJuego entity) => _session.Save(entity);
        public void Modify(PerfilJuego entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
