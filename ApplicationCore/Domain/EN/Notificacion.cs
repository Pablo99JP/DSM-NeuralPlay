using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Notificacion
    {
        public virtual long IdNotificacion { get; set; }
        public virtual TipoNotificacion Tipo { get; set; }
        public virtual string Mensaje { get; set; }
        public virtual bool Leida { get; set; }
        public virtual DateTime FechaCreacion { get; set; }
        
        public virtual Usuario Destinatario { get; set; }
        public virtual Publicacion OrigenPublicacion { get; set; }
        public virtual Comentario OrigenComentario { get; set; }
        public virtual PropuestaTorneo OrigenPropuesta { get; set; }
        public virtual Reaccion OrigenReaccion { get; set; }
    }
}
