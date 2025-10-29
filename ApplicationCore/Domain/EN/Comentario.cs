using System;

namespace ApplicationCore.Domain.EN
{
    public class Comentario
    {
        public virtual long IdComentario { get; set; }
        public virtual string Contenido { get; set; } = null!;
        public virtual DateTime FechaCreacion { get; set; }
        public virtual DateTime? FechaEdicion { get; set; }

        public virtual Usuario? Autor { get; set; }
        public virtual Publicacion? Publicacion { get; set; }
    }
}
