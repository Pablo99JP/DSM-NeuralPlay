using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IMiembroEquipoRepository : IRepository<MiembroEquipo>
    {
        IEnumerable<Usuario> GetUsuariosByEquipo(long idEquipo);
    }
}
