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

        public IEnumerable<PerfilJuego> ReadAll()
        {
            var q = _session.CreateQuery("from PerfilJuego");
            return q.List<PerfilJuego>();
        }

        public IEnumerable<PerfilJuego> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from PerfilJuego pj where pj.Juego is not null and lower(pj.Juego.NombreJuego) like :f");
            q.SetParameter("f", f);
            return q.List<PerfilJuego>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<PerfilJuego> BuscarPerfilesPorNombreJuego(string filtro) => ReadFilter(filtro);

        public void New(PerfilJuego entity) => _session.Save(entity);
        public void Modify(PerfilJuego entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
