using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface INotificacionRepository : IRepository<Notificacion, long>
    {
        IList<Notificacion> DamePorFiltro(string filtro);
    }
}
