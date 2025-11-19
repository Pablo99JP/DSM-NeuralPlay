using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace ApplicationCore.Domain.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        // Extensiones específicas de Usuario
        Usuario? ReadByNick(string nick);
        Usuario? ReadByEmail(string email);
    }
}
