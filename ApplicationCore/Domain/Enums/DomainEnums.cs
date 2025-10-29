namespace ApplicationCore.Domain.Enums
{
    public enum RolComunidad { LIDER, COLABORADOR, MIEMBRO }
    public enum RolEquipo { ADMIN, MIEMBRO }
    public enum EstadoMembresia { PENDIENTE, ACTIVA, EXPULSADA, ABANDONADA }
    public enum EstadoSolicitud { PENDIENTE, ACEPTADA, RECHAZADA, CANCELADA }
    public enum TipoInvitacion { COMUNIDAD, EQUIPO }
    public enum TipoReaccion { ME_GUSTA, OTRO }
    public enum EstadoParticipacion { PENDIENTE, ACEPTADA, RECHAZADA, RETIRADA }
    public enum TipoNotificacion { REACCION, COMENTARIO, PUBLICACION, PROPUESTA_TORNEO, UNION_TORNEO, SISTEMA, MENSAJE, ALERTA }
    public enum EstadoCuenta { ACTIVA }
    public enum Visibilidad { PUBLICO, PRIVADO, AMIGOS }
    public enum GeneroJuego { ACCION, AVENTURA, ESTRATEGIA, RPG, DEPORTE, OTRO }
}
