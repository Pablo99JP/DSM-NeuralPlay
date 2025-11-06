using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class ChatEquipoRepository : GenericRepository<ChatEquipo, long>, IChatEquipoRepository
    {
        public ChatEquipoRepository(ISession session) : base(session)
        {
        }
    }
}
