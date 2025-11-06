using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class ParticipacionTorneo
    {
        public virtual long IdParticipacion { get; set; }
        public virtual EstadoParticipacion Estado { get; set; }
        public virtual DateTime FechaAlta { get; set; }
        
        public virtual Equipo Equipo { get; set; }
        public virtual Torneo Torneo { get; set; }
    }
}
