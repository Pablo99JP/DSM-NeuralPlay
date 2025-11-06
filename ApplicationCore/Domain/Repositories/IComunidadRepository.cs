using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IComunidadRepository : IRepository<Comunidad, long>
    {
        IList<Comunidad> DamePorFiltro(string filtro);
    }
}
