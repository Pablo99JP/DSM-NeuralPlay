using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Usuario
    {
        public virtual long IdUsuario { get; set; }
        public virtual string Nick { get; set; } = null!;
        public virtual string CorreoElectronico { get; set; } = null!;
        public virtual string ContrasenaHash { get; set; } = null!;
        public virtual string? Telefono { get; set; }
        public virtual DateTime FechaRegistro { get; set; }
        public virtual EstadoCuenta EstadoCuenta { get; set; }

        // Navigation properties (lightweight)
        public virtual IList<MiembroComunidad> MiembrosComunidad { get; set; } = new List<MiembroComunidad>();
        public virtual IList<MiembroEquipo> MiembrosEquipo { get; set; } = new List<MiembroEquipo>();
        public virtual Perfil? Perfil { get; set; }
        public virtual IList<Sesion> Sesiones { get; set; } = new List<Sesion>();
    }
}
