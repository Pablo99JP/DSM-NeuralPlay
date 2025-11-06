using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IPerfilRepository : IRepository<Perfil, long>
    {
        IList<Perfil> DamePorFiltro(string filtro);
    }
}
