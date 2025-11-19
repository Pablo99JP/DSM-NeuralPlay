using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateChatEquipoRepository : IRepository<ChatEquipo>
    {
        private readonly ISession _session;

        public NHibernateChatEquipoRepository(ISession session)
        {
            _session = session;
        }

        public ChatEquipo? ReadById(long id) => _session.Get<ChatEquipo>(id);
        public IEnumerable<ChatEquipo> ReadAll()
        {
            var q = _session.CreateQuery("from ChatEquipo");
            return q.List<ChatEquipo>();
        }
        public IEnumerable<ChatEquipo> ReadFilter(string filter) => ReadAll();

        // Descriptive wrapper used by CENs
        public IEnumerable<ChatEquipo> BuscarChatsEquipo(string filtro) => ReadFilter(filtro);
        public void New(ChatEquipo entity) => _session.Save(entity);
        public void Modify(ChatEquipo entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }
    }
}
