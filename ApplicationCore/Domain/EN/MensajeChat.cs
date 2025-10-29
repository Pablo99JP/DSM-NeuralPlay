using System;

namespace ApplicationCore.Domain.EN
{
    public class MensajeChat
    {
        public virtual long IdMensajeChat { get; set; }
        public virtual string Contenido { get; set; } = null!;
        public virtual DateTime FechaEnvio { get; set; }

        public virtual ChatEquipo? Chat { get; set; }
        public virtual Usuario? Autor { get; set; }
    }
}
