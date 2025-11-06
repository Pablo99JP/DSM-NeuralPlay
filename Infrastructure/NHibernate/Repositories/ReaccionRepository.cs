using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class ReaccionRepository : GenericRepository<Reaccion, long>, IReaccionRepository
    {
        public ReaccionRepository(ISession session) : base(session)
        {
        }
    }
}
