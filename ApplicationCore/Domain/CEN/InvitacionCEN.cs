using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Invitacion.
    /// Expone operaciones CRUD para gestionar invitaciones a comunidades y equipos.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class InvitacionCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        // Permite testear este CEN con un mock, sin base de datos real
        private readonly IInvitacionRepository _invitacionRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// El framework DI resuelve automáticamente IInvitacionRepository.
        /// </summary>
        /// <param name="invitacionRepository">Implementación del repositorio de invitaciones</param>
        public InvitacionCEN(IInvitacionRepository invitacionRepository)
        {
            _invitacionRepository = invitacionRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva invitación.
        /// REGLA DE NEGOCIO: FechaEnvio se establece automáticamente a DateTime.Now.
        /// </summary>
        /// <param name="tipo">Tipo de invitación (COMUNIDAD o EQUIPO)</param>
        /// <param name="estado">Estado inicial (normalmente PENDIENTE)</param>
        /// <param name="fechaRespuesta">Fecha de respuesta (opcional)</param>
        /// <returns>ID de la invitación creada</returns>
        public long Crear(TipoInvitacion tipo, EstadoSolicitud estado, DateTime? fechaRespuesta = null)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var invitacion = new Invitacion
            {
                Tipo = tipo,
                Estado = estado,
                FechaEnvio = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaRespuesta = fechaRespuesta
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.New()
            _invitacionRepository.New(invitacion);
            
            return invitacion.IdInvitacion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una invitación existente.
        /// </summary>
        /// <param name="idInvitacion">ID de la invitación a modificar</param>
        /// <param name="tipo">Tipo de invitación</param>
        /// <param name="estado">Estado de la invitación</param>
        /// <param name="fechaEnvio">Fecha de envío</param>
        /// <param name="fechaRespuesta">Fecha de respuesta (opcional)</param>
        public void Modificar(long idInvitacion, TipoInvitacion tipo, EstadoSolicitud estado, 
            DateTime fechaEnvio, DateTime? fechaRespuesta = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.DamePorOID()
            var invitacion = _invitacionRepository.DamePorOID(idInvitacion);
            
            // Actualiza las propiedades de la entidad (NHibernate detecta cambios automáticamente)
            invitacion.Tipo = tipo;
            invitacion.Estado = estado;
            invitacion.FechaEnvio = fechaEnvio;
            invitacion.FechaRespuesta = fechaRespuesta;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.Modify()
            _invitacionRepository.Modify(invitacion);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una invitación por su ID.
        /// </summary>
        /// <param name="idInvitacion">ID de la invitación a eliminar</param>
        public void Eliminar(long idInvitacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.Destroy()
            // Ejecuta: DELETE FROM Invitacion WHERE IdInvitacion = @idInvitacion
            _invitacionRepository.Destroy(idInvitacion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una invitación por su identificador único.
        /// </summary>
        /// <param name="idInvitacion">ID de la invitación</param>
        /// <returns>Entidad Invitacion o null si no existe</returns>
        public Invitacion DamePorOID(long idInvitacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.DamePorOID()
            return _invitacionRepository.DamePorOID(idInvitacion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las invitaciones del sistema.
        /// </summary>
        /// <returns>Lista de todas las invitaciones</returns>
        public IList<Invitacion> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → GenericRepository.DameTodos()
            return _invitacionRepository.DameTodos();
        }
    }
}
