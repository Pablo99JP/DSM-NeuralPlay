using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface ITorneoRepository : IRepository<Torneo, long>
    {
        IList<Torneo> DamePorFiltro(string filtro);
        IList<Torneo> DamePorEquipo(long idEquipo);
    }
}
