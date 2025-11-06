using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class PropuestaTorneoRepository : GenericRepository<PropuestaTorneo, long>, IPropuestaTorneoRepository
    {
        public PropuestaTorneoRepository(ISession session) : base(session)
        {
        }
    }
}
