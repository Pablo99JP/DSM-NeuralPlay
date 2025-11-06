using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Equipo
    {
        public virtual long IdEquipo { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Descripcion { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        
        public virtual Comunidad Comunidad { get; set; }
        public virtual ICollection<MiembroEquipo> Miembros { get; set; }
        public virtual ChatEquipo Chat { get; set; }
        public virtual ICollection<Invitacion> Invitaciones { get; set; }
        public virtual ICollection<SolicitudIngreso> Solicitudes { get; set; }
        public virtual ICollection<PropuestaTorneo> PropuestasTorneo { get; set; }
        public virtual ICollection<ParticipacionTorneo> Participaciones { get; set; }
        
        public Equipo()
        {
            Miembros = new List<MiembroEquipo>();
            Invitaciones = new List<Invitacion>();
            Solicitudes = new List<SolicitudIngreso>();
            PropuestasTorneo = new List<PropuestaTorneo>();
            Participaciones = new List<ParticipacionTorneo>();
        }
    }
}
