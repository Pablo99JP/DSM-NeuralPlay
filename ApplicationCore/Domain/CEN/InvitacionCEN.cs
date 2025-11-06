using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.CEN
{
    public class InvitacionCEN
    {
        private readonly IRepository<Invitacion> _repo;

        public InvitacionCEN(IRepository<Invitacion> repo)
        {
            _repo = repo;
        }

        public Invitacion NewInvitacion(TipoInvitacion tipo, Usuario emisor, Usuario destinatario, Comunidad? comunidad = null, Equipo? equipo = null)
        {
            var i = new Invitacion 
            { 
                Tipo = tipo, 
                Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE, 
                FechaEnvio = System.DateTime.UtcNow,
                Emisor = emisor,
                Destinatario = destinatario,
                Comunidad = comunidad,
                Equipo = equipo
            };
            _repo.New(i);
            return i;
        }

        public Invitacion? ReadOID_Invitacion(long id) => _repo.ReadById(id);
        public IEnumerable<Invitacion> ReadAll_Invitacion() => _repo.ReadAll();
        public void ModifyInvitacion(Invitacion i) => _repo.Modify(i);
        public void DestroyInvitacion(long id) => _repo.Destroy(id);
        public IEnumerable<Invitacion> ReadFilter_Invitacion(string filter) => _repo.ReadFilter(filter);
    }
}
