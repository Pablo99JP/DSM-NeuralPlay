using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    public class NotificacionRepository : GenericRepository<Notificacion, long>, INotificacionRepository
    {
        public NotificacionRepository(ISession session) : base(session)
        {
        }

        public IList<Notificacion> DamePorFiltro(string filtro)
        {
            return _session.Query<Notificacion>()
                .Where(n => n.Mensaje.Contains(filtro))
                .ToList();
        }
    }
}
