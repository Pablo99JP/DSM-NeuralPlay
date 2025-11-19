using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateJuegoRepository : IRepository<Juego>
    {
        private readonly ISession _session;

        public NHibernateJuegoRepository(ISession session)
        {
            _session = session;
        }

        public Juego? ReadById(long id) => _session.Get<Juego>(id);

        public IEnumerable<Juego> ReadAll()
        {
            var q = _session.CreateQuery("from Juego");
            return q.List<Juego>();
        }

        public IEnumerable<Juego> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from Juego j where lower(j.NombreJuego) like :f");
            q.SetParameter("f", f);
            return q.List<Juego>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<Juego> BuscarJuegosPorNombreJuego(string filtro) => ReadFilter(filtro);

        public void New(Juego entity) => _session.Save(entity);
        public void Modify(Juego entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
