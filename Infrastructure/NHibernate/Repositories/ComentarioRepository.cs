using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class ComentarioRepository : GenericRepository<Comentario, long>, IComentarioRepository
    {
        public ComentarioRepository(ISession session) : base(session)
        {
        }
    }
}
