using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class ChatEquipoCEN
    {
        private readonly IRepository<ChatEquipo> _repo;

        public ChatEquipoCEN(IRepository<ChatEquipo> repo)
        {
            _repo = repo;
        }

        public ChatEquipo NewChatEquipo(Equipo equipo)
        {
            var c = new ChatEquipo { };
            _repo.New(c);
            return c;
        }

        public ChatEquipo? ReadOID_ChatEquipo(long id) => _repo.ReadById(id);
        public IEnumerable<ChatEquipo> ReadAll_ChatEquipo() => _repo.ReadAll();
        public void ModifyChatEquipo(ChatEquipo c) => _repo.Modify(c);
        public void DestroyChatEquipo(long id) => _repo.Destroy(id);
        public IEnumerable<ChatEquipo> BuscarChatsEquipo(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarChatsEquipo");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<ChatEquipo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}
