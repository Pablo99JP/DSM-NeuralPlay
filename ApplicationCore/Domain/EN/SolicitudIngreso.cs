using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class SolicitudIngreso
    {
        public virtual long IdSolicitud { get; set; }
        public virtual TipoInvitacion Tipo { get; set; }
        public virtual EstadoSolicitud Estado { get; set; }
        public virtual DateTime FechaSolicitud { get; set; }
        public virtual DateTime? FechaResolucion { get; set; }

        public virtual Usuario? Solicitante { get; set; }
        public virtual Comunidad? Comunidad { get; set; }
        public virtual Equipo? Equipo { get; set; }
    }
}
