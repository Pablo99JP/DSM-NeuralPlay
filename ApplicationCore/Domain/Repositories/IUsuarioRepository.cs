using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario, long>
    {
        IList<Usuario> DamePorFiltro(string filtro);
        IList<Usuario> DamePorEquipo(long idEquipo);
        IList<Usuario> DamePorComunidad(long idComunidad);
    }
}
