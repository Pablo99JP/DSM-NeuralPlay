using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class MensajeChatRepository : GenericRepository<MensajeChat, long>, IMensajeChatRepository
    {
        public MensajeChatRepository(ISession session) : base(session)
        {
        }
    }
}
