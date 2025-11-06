using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Torneo
    {
        public virtual long IdTorneo { get; set; }
        public virtual string Nombre { get; set; }
        public virtual DateTime FechaInicio { get; set; }
        public virtual string Reglas { get; set; }
        public virtual string Estado { get; set; }
        
        public virtual Comunidad Comunidad { get; set; }
        public virtual ICollection<PropuestaTorneo> Propuestas { get; set; }
        public virtual ICollection<ParticipacionTorneo> Participaciones { get; set; }
        
        public Torneo()
        {
            Propuestas = new List<PropuestaTorneo>();
            Participaciones = new List<ParticipacionTorneo>();
        }
    }
}
