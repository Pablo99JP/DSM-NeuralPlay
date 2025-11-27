using System.Collections.Generic;
using NHibernate;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace Infrastructure.NHibernate
{
    public class NHibernateMensajeChatRepository : IRepository<MensajeChat>
    {
        private readonly ISession _session;

        public NHibernateMensajeChatRepository(ISession session)
        {
            _session = session;
        }

        public MensajeChat? ReadById(long id) => _session.Get<MensajeChat>(id);

        public IEnumerable<MensajeChat> ReadAll()
        {
            var q = _session.CreateQuery("from MensajeChat");
            return q.List<MensajeChat>();
        }

        public IEnumerable<MensajeChat> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            var f = "%" + filter.ToLowerInvariant() + "%";
            var q = _session.CreateQuery("from MensajeChat m where lower(m.Contenido) like :f");
            q.SetParameter("f", f);
            return q.List<MensajeChat>();
        }

        // Descriptive wrapper used by CENs
        public IEnumerable<MensajeChat> BuscarMensajesChatPorContenido(string filtro) => ReadFilter(filtro);

        public void New(MensajeChat entity) => _session.Save(entity);
        public void Modify(MensajeChat entity) => _session.Update(entity);
        public void Destroy(long id)
        {
            var e = ReadById(id);
            if (e != null) _session.Delete(e);
        }

        // NUEVO: lectura directa por ChatId vía HQL
        public IEnumerable<MensajeChat> ReadByChatId(long chatId)
        {
            var q = _session.CreateQuery("from MensajeChat m where m.Chat.IdChatEquipo = :id");
            q.SetParameter("id", chatId);
            return q.List<MensajeChat>();
        }
    }
}
