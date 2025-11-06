using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class PublicacionRepository : GenericRepository<Publicacion, long>, IPublicacionRepository
    {
        public PublicacionRepository(ISession session) : base(session)
        {
        }

        public IList<Publicacion> DamePorFiltro(string filtro)
        {
            return _session.Query<Publicacion>()
                .Where(p => p.Contenido.Contains(filtro))
                .ToList();
        }
    }
}
