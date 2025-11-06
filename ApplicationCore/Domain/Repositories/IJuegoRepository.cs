using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IJuegoRepository : IRepository<Juego, long>
    {
        IList<Juego> DamePorFiltro(string filtro);
    }
}
