using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class SesionRepository : GenericRepository<Sesion, long>, ISesionRepository
    {
        public SesionRepository(ISession session) : base(session)
        {
        }
    }
}
