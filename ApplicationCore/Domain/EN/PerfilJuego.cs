using System;

namespace ApplicationCore.Domain.EN
{
    public class PerfilJuego
    {
        public virtual long IdPerfilJuego { get; set; }
        public virtual DateTime FechaAdicion { get; set; }

        public virtual Perfil Perfil { get; set; } = null!;
        public virtual Juego Juego { get; set; } = null!;
    }
}
