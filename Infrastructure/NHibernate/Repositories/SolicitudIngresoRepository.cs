using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class SolicitudIngresoRepository : GenericRepository<SolicitudIngreso, long>, ISolicitudIngresoRepository
    {
        public SolicitudIngresoRepository(ISession session) : base(session)
        {
        }
    }
}
