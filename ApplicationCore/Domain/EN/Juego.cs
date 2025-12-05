using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Juego
    {
        public virtual long IdJuego { get; set; }
        public virtual string NombreJuego { get; set; } = null!;
        public virtual GeneroJuego Genero { get; set; }
        public virtual string? ImagenUrl { get; set; }
        public virtual string? Descripcion { get; set; }

        public virtual IList<PerfilJuego> PerfilJuegos { get; set; } = new List<PerfilJuego>();
    }
}
