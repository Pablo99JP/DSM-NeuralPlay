using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Reaccion
    {
        public virtual long IdReaccion { get; set; }
        public virtual TipoReaccion Tipo { get; set; }
        public virtual DateTime FechaCreacion { get; set; }

        public virtual Usuario? Autor { get; set; }
        public virtual Publicacion? Publicacion { get; set; }
        public virtual Comentario? Comentario { get; set; }
    }
}
