using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Perfil
    {
        public virtual long IdPerfil { get; set; }
        public virtual string FotoPerfilUrl { get; set; }
        public virtual string Descripcion { get; set; }
        public virtual Visibilidad VisibilidadPerfil { get; set; }
        public virtual Visibilidad VisibilidadActividad { get; set; }
        public virtual long? JuegoFavoritoId { get; set; }
        
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<PerfilJuego> PerfilJuegos { get; set; }
        public virtual Juego JuegoFavorito { get; set; }
        
        public Perfil()
        {
            PerfilJuegos = new List<PerfilJuego>();
        }
    }
}
