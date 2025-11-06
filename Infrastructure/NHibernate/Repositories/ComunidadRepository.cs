using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class ComunidadRepository : GenericRepository<Comunidad, long>, IComunidadRepository
    {
        public ComunidadRepository(ISession session) : base(session)
        {
        }

        public IList<Comunidad> DamePorFiltro(string filtro)
        {
            return _session.Query<Comunidad>()
                .Where(c => c.Nombre.Contains(filtro) || c.Descripcion.Contains(filtro))
                .ToList();
        }
    }
}
