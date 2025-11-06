using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class ParticipacionTorneoRepository : GenericRepository<ParticipacionTorneo, long>, IParticipacionTorneoRepository
    {
        public ParticipacionTorneoRepository(ISession session) : base(session)
        {
        }
    }
}
