using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Juego
    {
        public virtual long IdJuego { get; set; }
        public virtual string NombreJuego { get; set; }
        public virtual GeneroJuego Genero { get; set; }
        
        public virtual ICollection<PerfilJuego> PerfilJuegos { get; set; }
        
        public Juego()
        {
            PerfilJuegos = new List<PerfilJuego>();
        }
    }
}
