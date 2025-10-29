using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class MensajeChatCEN
    {
        private readonly IRepository<MensajeChat> _repo;

        public MensajeChatCEN(IRepository<MensajeChat> repo)
        {
            _repo = repo;
        }

        public MensajeChat NewMensajeChat(string contenido, Usuario autor, ChatEquipo chat)
        {
            var m = new MensajeChat { Contenido = contenido, FechaEnvio = System.DateTime.UtcNow, Autor = autor, Chat = chat };
            _repo.New(m);
            return m;
        }

        public MensajeChat? ReadOID_MensajeChat(long id) => _repo.ReadById(id);
        public IEnumerable<MensajeChat> ReadAll_MensajeChat() => _repo.ReadAll();
        public void ModifyMensajeChat(MensajeChat m) => _repo.Modify(m);
        public void DestroyMensajeChat(long id) => _repo.Destroy(id);
        public IEnumerable<MensajeChat> ReadFilter_MensajeChat(string filter) => _repo.ReadFilter(filter);
    }
}
