using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class MiembroComunidad
    {
        public virtual long IdMiembroComunidad { get; set; }
        public virtual RolComunidad Rol { get; set; }
        public virtual EstadoMembresia Estado { get; set; }
        public virtual DateTime FechaAlta { get; set; }
        public virtual DateTime? FechaBaja { get; set; }
        
        public virtual Usuario Usuario { get; set; }
        public virtual Comunidad Comunidad { get; set; }
    }
}
