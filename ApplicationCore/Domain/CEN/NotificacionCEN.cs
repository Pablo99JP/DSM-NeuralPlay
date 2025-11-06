using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Notificacion.
    /// Expone operaciones CRUD para gestionar notificaciones de usuarios.
    /// Las notificaciones informan sobre reacciones, comentarios, propuestas, etc.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class NotificacionCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly INotificacionRepository _notificacionRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="notificacionRepository">Implementación del repositorio de notificaciones</param>
        public NotificacionCEN(INotificacionRepository notificacionRepository)
        {
            _notificacionRepository = notificacionRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva notificación.
        /// REGLA DE NEGOCIO: FechaCreacion se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: Leida se establece inicialmente en false.
        /// </summary>
        /// <param name="tipo">Tipo de notificación (REACCION, COMENTARIO, PUBLICACION, etc.)</param>
        /// <param name="mensaje">Mensaje de la notificación (obligatorio)</param>
        /// <returns>ID de la notificación creada</returns>
        public long Crear(TipoNotificacion tipo, string mensaje)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var notificacion = new Notificacion
            {
                Tipo = tipo,
                Mensaje = mensaje,
                FechaCreacion = DateTime.Now,  // ← REGLA: Siempre fecha actual
                Leida = false                   // ← REGLA: Inicialmente no leída
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.New()
            _notificacionRepository.New(notificacion);
            
            return notificacion.IdNotificacion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una notificación existente.
        /// Se usa principalmente para marcar notificaciones como leídas.
        /// </summary>
        /// <param name="idNotificacion">ID de la notificación a modificar</param>
        /// <param name="tipo">Tipo de notificación</param>
        /// <param name="mensaje">Mensaje de la notificación</param>
        /// <param name="leida">Estado de lectura</param>
        /// <param name="fechaCreacion">Fecha de creación</param>
        public void Modificar(long idNotificacion, TipoNotificacion tipo, string mensaje, bool leida, DateTime fechaCreacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.DamePorOID()
            var notificacion = _notificacionRepository.DamePorOID(idNotificacion);
            
            // Actualiza las propiedades
            notificacion.Tipo = tipo;
            notificacion.Mensaje = mensaje;
            notificacion.Leida = leida;
            notificacion.FechaCreacion = fechaCreacion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.Modify()
            _notificacionRepository.Modify(notificacion);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una notificación por su ID.
        /// </summary>
        /// <param name="idNotificacion">ID de la notificación a eliminar</param>
        public void Eliminar(long idNotificacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.Destroy()
            _notificacionRepository.Destroy(idNotificacion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una notificación por su identificador único.
        /// </summary>
        /// <param name="idNotificacion">ID de la notificación</param>
        /// <returns>Entidad Notificacion o null si no existe</returns>
        public Notificacion DamePorOID(long idNotificacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.DamePorOID()
            return _notificacionRepository.DamePorOID(idNotificacion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las notificaciones del sistema.
        /// </summary>
        /// <returns>Lista de todas las notificaciones</returns>
        public IList<Notificacion> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.DameTodos()
            return _notificacionRepository.DameTodos();
        }

        /// <summary>
        /// [CUSTOM METHOD] Marca una notificación como leída.
        /// Método de conveniencia para el caso de uso más común.
        /// </summary>
        /// <param name="idNotificacion">ID de la notificación a marcar como leída</param>
        public void MarcarComoLeida(long idNotificacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.DamePorOID()
            var notificacion = _notificacionRepository.DamePorOID(idNotificacion);
            
            // REGLA: Cambiar estado de Leida a true
            notificacion.Leida = true;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/NotificacionRepository.cs → GenericRepository.Modify()
            _notificacionRepository.Modify(notificacion);
        }
    }
}
