using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class PerfilRepository : GenericRepository<Perfil, long>, IPerfilRepository
    {
        public PerfilRepository(ISession session) : base(session)
        {
        }

        public IList<Perfil> DamePorFiltro(string filtro)
        {
            return _session.Query<Perfil>()
                .Where(p => p.Descripcion.Contains(filtro))
                .ToList();
        }
    }
}
