using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Publicacion
    {
        public virtual long IdPublicacion { get; set; }
        public virtual string Contenido { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        public virtual DateTime? FechaEdicion { get; set; }
        
        public virtual Comunidad Comunidad { get; set; }
        public virtual Usuario Autor { get; set; }
        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<Reaccion> Reacciones { get; set; }
        public virtual ICollection<Notificacion> Notificaciones { get; set; }
        
        public Publicacion()
        {
            Comentarios = new List<Comentario>();
            Reacciones = new List<Reaccion>();
            Notificaciones = new List<Notificacion>();
        }
    }
}
