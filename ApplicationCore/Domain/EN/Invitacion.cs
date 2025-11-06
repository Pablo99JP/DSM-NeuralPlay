using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Invitacion
    {
        public virtual long IdInvitacion { get; set; }
        public virtual TipoInvitacion Tipo { get; set; }
        public virtual EstadoSolicitud Estado { get; set; }
        public virtual DateTime FechaEnvio { get; set; }
        public virtual DateTime? FechaRespuesta { get; set; }
        
        public virtual Usuario Emisor { get; set; }
        public virtual Usuario Destinatario { get; set; }
        public virtual Comunidad Comunidad { get; set; }
        public virtual Equipo Equipo { get; set; }
    }
}
