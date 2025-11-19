using System;

namespace ApplicationCore.Domain.EN
{
    public class VotoTorneo
    {
        public virtual long IdVoto { get; set; }
        public virtual bool Valor { get; set; }
        public virtual DateTime FechaVoto { get; set; }

        public virtual Usuario? Votante { get; set; }
        public virtual PropuestaTorneo? Propuesta { get; set; }
    }
}
