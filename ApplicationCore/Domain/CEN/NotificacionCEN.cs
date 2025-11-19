using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class NotificacionCEN
    {
        private readonly IRepository<Notificacion> _repo;

        public NotificacionCEN(IRepository<Notificacion> repo)
        {
            _repo = repo;
        }

        public Notificacion NewNotificacion(TipoNotificacion tipo, string mensaje, Usuario destinatario)
        {
            var n = new Notificacion { Tipo = tipo, Mensaje = mensaje, Leida = false, FechaCreacion = System.DateTime.UtcNow, Destinatario = destinatario };
            _repo.New(n);
            return n;
        }

        public Notificacion? ReadOID_Notificacion(long id) => _repo.ReadById(id);
        public IEnumerable<Notificacion> ReadAll_Notificacion() => _repo.ReadAll();
        public void ModifyNotificacion(Notificacion n) => _repo.Modify(n);
        public void DestroyNotificacion(long id) => _repo.Destroy(id);
        public IEnumerable<Notificacion> BuscarNotificacionesPorMensajeODestinatarioNick(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarNotificacionesPorMensajeODestinatarioNick");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<Notificacion> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}
