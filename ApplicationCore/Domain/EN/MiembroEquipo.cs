using System;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class MiembroEquipo
    {
        public virtual long IdMiembroEquipo { get; set; }
        public virtual RolEquipo Rol { get; set; }
        public virtual EstadoMembresia Estado { get; set; }
        public virtual DateTime FechaAlta { get; set; }
        public virtual DateTime? FechaBaja { get; set; }
        
        public virtual Usuario Usuario { get; set; }
        public virtual Equipo Equipo { get; set; }
    }
}
