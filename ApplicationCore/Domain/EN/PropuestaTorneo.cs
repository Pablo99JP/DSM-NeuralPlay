using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class PropuestaTorneo
    {
        public virtual long IdPropuesta { get; set; }
        public virtual DateTime FechaPropuesta { get; set; }
        public virtual EstadoSolicitud Estado { get; set; }
        
        public virtual Equipo Equipo { get; set; }
        public virtual Torneo Torneo { get; set; }
        public virtual Usuario PropuestoPor { get; set; }
        public virtual ICollection<VotoTorneo> Votos { get; set; }
        public virtual ICollection<Notificacion> Notificaciones { get; set; }
        
        public PropuestaTorneo()
        {
            Votos = new List<VotoTorneo>();
            Notificaciones = new List<Notificacion>();
        }
    }
}
