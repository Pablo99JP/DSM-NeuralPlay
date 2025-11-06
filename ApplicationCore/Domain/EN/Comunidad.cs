using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Comunidad
    {
        public virtual long IdComunidad { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Descripcion { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        
        public virtual ICollection<MiembroComunidad> Miembros { get; set; }
        public virtual ICollection<Equipo> Equipos { get; set; }
        public virtual ICollection<Torneo> Torneos { get; set; }
        public virtual ICollection<Invitacion> Invitaciones { get; set; }
        public virtual ICollection<SolicitudIngreso> Solicitudes { get; set; }
        public virtual ICollection<Publicacion> Publicaciones { get; set; }
        
        public Comunidad()
        {
            Miembros = new List<MiembroComunidad>();
            Equipos = new List<Equipo>();
            Torneos = new List<Torneo>();
            Invitaciones = new List<Invitacion>();
            Solicitudes = new List<SolicitudIngreso>();
            Publicaciones = new List<Publicacion>();
        }
    }
}
