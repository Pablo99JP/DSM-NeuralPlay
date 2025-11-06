using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IPublicacionRepository : IRepository<Publicacion, long>
    {
        IList<Publicacion> DamePorFiltro(string filtro);
    }
}
