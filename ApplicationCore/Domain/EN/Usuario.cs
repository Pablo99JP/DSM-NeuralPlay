using System;
using System.Collections.Generic;
using ApplicationCore.Domain.Enums;

namespace ApplicationCore.Domain.EN
{
    public class Usuario
    {
        public virtual long IdUsuario { get; set; }
        public virtual string Nick { get; set; }
        public virtual string CorreoElectronico { get; set; }
        public virtual string ContrasenaHash { get; set; }
        public virtual string Telefono { get; set; }
        public virtual DateTime FechaRegistro { get; set; }
        public virtual EstadoCuenta EstadoCuenta { get; set; }
        
        public virtual ICollection<MiembroComunidad> MiembrosComunidad { get; set; }
        public virtual ICollection<MiembroEquipo> MiembrosEquipo { get; set; }
        public virtual ICollection<MensajeChat> MensajesChat { get; set; }
        public virtual ICollection<Invitacion> InvitacionesEmitidas { get; set; }
        public virtual ICollection<Invitacion> InvitacionesRecibidas { get; set; }
        public virtual ICollection<SolicitudIngreso> Solicitudes { get; set; }
        public virtual ICollection<Publicacion> Publicaciones { get; set; }
        public virtual ICollection<Comentario> Comentarios { get; set; }
        public virtual ICollection<Reaccion> Reacciones { get; set; }
        public virtual ICollection<Notificacion> Notificaciones { get; set; }
        public virtual ICollection<PropuestaTorneo> PropuestasTorneo { get; set; }
        public virtual ICollection<VotoTorneo> VotosTorneo { get; set; }
        public virtual Perfil Perfil { get; set; }
        public virtual ICollection<Sesion> Sesiones { get; set; }
        
        public Usuario()
        {
            MiembrosComunidad = new List<MiembroComunidad>();
            MiembrosEquipo = new List<MiembroEquipo>();
            MensajesChat = new List<MensajeChat>();
            InvitacionesEmitidas = new List<Invitacion>();
            InvitacionesRecibidas = new List<Invitacion>();
            Solicitudes = new List<SolicitudIngreso>();
            Publicaciones = new List<Publicacion>();
            Comentarios = new List<Comentario>();
            Reacciones = new List<Reaccion>();
            Notificaciones = new List<Notificacion>();
            PropuestasTorneo = new List<PropuestaTorneo>();
            VotosTorneo = new List<VotoTorneo>();
            Sesiones = new List<Sesion>();
        }
    }
}
