using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Comentario
    {
        public virtual long IdComentario { get; set; }
        public virtual string Contenido { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        public virtual DateTime? FechaEdicion { get; set; }
        
        public virtual Publicacion Publicacion { get; set; }
        public virtual Usuario Autor { get; set; }
        public virtual ICollection<Reaccion> Reacciones { get; set; }
        public virtual ICollection<Notificacion> Notificaciones { get; set; }
        
        public Comentario()
        {
            Reacciones = new List<Reaccion>();
            Notificaciones = new List<Notificacion>();
        }
    }
}
