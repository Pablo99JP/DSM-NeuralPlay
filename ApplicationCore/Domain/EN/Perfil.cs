using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Perfil
    {
        public virtual long IdPerfil { get; set; }
        public virtual string? FotoPerfilUrl { get; set; }
        public virtual string? Descripcion { get; set; }
        public virtual Visibilidad VisibilidadPerfil { get; set; }
        public virtual Visibilidad VisibilidadActividad { get; set; }
        
        // --- INICIO DE LA CORRECCIÓN ---
        // Se añade la propiedad de navegación para el juego favorito.
        // Esto no afecta a la lista 'PerfilJuegos'.
        public virtual Juego? JuegoFavorito { get; set; }
        // --- FIN DE LA CORRECCIÓN ---

        public virtual Usuario Usuario { get; set; } = null!;
        
        // La lista de juegos del perfil se mantiene intacta.
        public virtual IList<PerfilJuego> PerfilJuegos { get; set; } = new List<PerfilJuego>();
    }
}
