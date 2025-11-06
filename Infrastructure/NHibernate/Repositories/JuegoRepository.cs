using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class JuegoRepository : GenericRepository<Juego, long>, IJuegoRepository
    {
        public JuegoRepository(ISession session) : base(session)
        {
        }

        public IList<Juego> DamePorFiltro(string filtro)
        {
            return _session.Query<Juego>()
                .Where(j => j.NombreJuego.Contains(filtro))
                .ToList();
        }
    }
}
