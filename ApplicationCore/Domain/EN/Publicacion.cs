using System;
using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class Publicacion
    {
        public virtual long IdPublicacion { get; set; }
        public virtual string Contenido { get; set; } = null!;
        public virtual DateTime FechaCreacion { get; set; }
        public virtual DateTime? FechaEdicion { get; set; }

        public virtual Comunidad? Comunidad { get; set; }
        public virtual Usuario? Autor { get; set; }
        public virtual IList<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public virtual IList<Reaccion> Reacciones { get; set; } = new List<Reaccion>();
    }
}
