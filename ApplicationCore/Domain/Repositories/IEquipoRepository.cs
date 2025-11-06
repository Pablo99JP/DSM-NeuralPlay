using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IEquipoRepository : IRepository<Equipo, long>
    {
        IList<Equipo> DamePorFiltro(string filtro);
        IList<Equipo> DamePorTorneo(long idTorneo);
    }
}
