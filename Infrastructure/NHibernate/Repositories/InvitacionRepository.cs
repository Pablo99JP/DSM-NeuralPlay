using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class InvitacionRepository : GenericRepository<Invitacion, long>, IInvitacionRepository
    {
        public InvitacionRepository(ISession session) : base(session)
        {
        }
    }
}
