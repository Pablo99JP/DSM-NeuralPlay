using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Comunidad
    {
        public virtual long IdComunidad { get; set; }
        public virtual string Nombre { get; set; } = null!;
        public virtual string? Descripcion { get; set; }
        public virtual DateTime FechaCreacion { get; set; }

        public virtual IList<Equipo> Equipos { get; set; } = new List<Equipo>();
        public virtual IList<Torneo> Torneos { get; set; } = new List<Torneo>();
        public virtual IList<MiembroComunidad> Miembros { get; set; } = new List<MiembroComunidad>();
        public virtual IList<Publicacion> Publicaciones { get; set; } = new List<Publicacion>();
    }
}
