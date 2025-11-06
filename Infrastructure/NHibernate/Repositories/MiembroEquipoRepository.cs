using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class MiembroEquipoRepository : GenericRepository<MiembroEquipo, long>, IMiembroEquipoRepository
    {
        public MiembroEquipoRepository(ISession session) : base(session)
        {
        }
    }
}
