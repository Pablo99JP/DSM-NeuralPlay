using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class MiembroComunidadRepository : GenericRepository<MiembroComunidad, long>, IMiembroComunidadRepository
    {
        public MiembroComunidadRepository(ISession session) : base(session)
        {
        }
    }
}
