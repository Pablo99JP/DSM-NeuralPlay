using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Torneo
    {
        public virtual long IdTorneo { get; set; }
        public virtual string Nombre { get; set; } = null!;
        public virtual DateTime FechaInicio { get; set; }
        public virtual string? Reglas { get; set; }
        public virtual string Estado { get; set; } = null!;

        public virtual Usuario? Creador { get; set; }
        public virtual Comunidad? ComunidadOrganizadora { get; set; }
        public virtual IList<PropuestaTorneo> Propuestas { get; set; } = new List<PropuestaTorneo>();
        public virtual IList<ParticipacionTorneo> Participaciones { get; set; } = new List<ParticipacionTorneo>();
    }
}
