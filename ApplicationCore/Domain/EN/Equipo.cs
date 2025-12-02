using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Equipo
    {
        public virtual long IdEquipo { get; set; }
        public virtual string Nombre { get; set; } = null!;
        public virtual string? Descripcion { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        public virtual Actividad Actividad { get; set; }
        public virtual string? Pais { get; set; }
        public virtual string? Idioma { get; set; }

        public virtual Comunidad? Comunidad { get; set; }
        public virtual IList<MiembroEquipo> Miembros { get; set; } = new List<MiembroEquipo>();
        public virtual ChatEquipo? Chat { get; set; }
        public virtual IList<PropuestaTorneo> PropuestasTorneo { get; set; } = new List<PropuestaTorneo>();
    }
}
