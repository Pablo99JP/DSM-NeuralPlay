using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class VotoTorneoRepository : GenericRepository<VotoTorneo, long>, IVotoTorneoRepository
    {
        public VotoTorneoRepository(ISession session) : base(session)
        {
        }
    }
}
