using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class PerfilJuegoRepository : GenericRepository<PerfilJuego, long>, IPerfilJuegoRepository
    {
        public PerfilJuegoRepository(ISession session) : base(session)
        {
        }
    }
}
