using System;

namespace ApplicationCore.Domain.EN
{
    public class Sesion
    {
        public virtual long IdSesion { get; set; }
        public virtual DateTime FechaInicio { get; set; }
        public virtual DateTime? FechaFin { get; set; }
        public virtual string Token { get; set; }
        
        public virtual Usuario Usuario { get; set; }
    }
}
