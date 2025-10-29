using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    public class InMemoryUsuarioRepository : InMemoryRepository<Usuario>, IUsuarioRepository
    {
        public Usuario? ReadByNick(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return null;
            return ReadAll().FirstOrDefault(u => u.Nick?.ToLowerInvariant() == nick.ToLowerInvariant());
        }

        public override IEnumerable<Usuario> ReadFilter(string filter)
        {
            return base.ReadFilter(filter);
        }
    }
}
