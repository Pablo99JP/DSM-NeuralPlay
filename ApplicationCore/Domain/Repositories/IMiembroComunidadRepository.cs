using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace ApplicationCore.Domain.Repositories
{
    public interface IMiembroComunidadRepository : IRepository<MiembroComunidad>
    {
        IEnumerable<Usuario> GetUsuariosByComunidad(long idComunidad);
    }
}
