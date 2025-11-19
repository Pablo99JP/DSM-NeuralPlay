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
        public virtual long? JuegoFavoritoId { get; set; }

        public virtual Usuario Usuario { get; set; } = null!;
        public virtual IList<PerfilJuego> PerfilJuegos { get; set; } = new List<PerfilJuego>();
    }
}
