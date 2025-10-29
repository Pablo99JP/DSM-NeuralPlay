using System;

namespace ApplicationCore.Domain.EN
{
    public class ParticipacionTorneo
    {
        public virtual long IdParticipacion { get; set; }
        public virtual string Estado { get; set; } = null!; // EstadoParticipacion
        public virtual DateTime FechaAlta { get; set; }

        public virtual Equipo Equipo { get; set; } = null!;
        public virtual Torneo Torneo { get; set; } = null!;
    }
}
