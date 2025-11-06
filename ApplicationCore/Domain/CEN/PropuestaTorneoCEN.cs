using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad PropuestaTorneo.
    /// Expone operaciones CRUD para gestionar propuestas de equipos para unirse a torneos.
    /// REGLA IMPORTANTE: Si todos los miembros del equipo votan "sí", el equipo se une al torneo.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class PropuestaTorneoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IPropuestaTorneoRepository _propuestaTorneoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="propuestaTorneoRepository">Implementación del repositorio de propuestas de torneo</param>
        public PropuestaTorneoCEN(IPropuestaTorneoRepository propuestaTorneoRepository)
        {
            _propuestaTorneoRepository = propuestaTorneoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva propuesta de torneo.
        /// REGLA DE NEGOCIO: FechaPropuesta se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: Estado inicial suele ser PENDIENTE.
        /// </summary>
        /// <param name="estado">Estado inicial de la propuesta (normalmente PENDIENTE)</param>
        /// <returns>ID de la propuesta creada</returns>
        public long Crear(EstadoSolicitud estado)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var propuestaTorneo = new PropuestaTorneo
            {
                FechaPropuesta = DateTime.Now,  // ← REGLA: Siempre fecha actual
                Estado = estado
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.New()
            _propuestaTorneoRepository.New(propuestaTorneo);
            
            return propuestaTorneo.IdPropuesta;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una propuesta de torneo existente.
        /// Se usa para actualizar el estado según los votos recibidos.
        /// </summary>
        /// <param name="idPropuesta">ID de la propuesta a modificar</param>
        /// <param name="fechaPropuesta">Fecha de la propuesta</param>
        /// <param name="estado">Estado de la propuesta (PENDIENTE, ACEPTADA, RECHAZADA, CANCELADA)</param>
        public void Modificar(long idPropuesta, DateTime fechaPropuesta, EstadoSolicitud estado)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.DamePorOID()
            var propuestaTorneo = _propuestaTorneoRepository.DamePorOID(idPropuesta);
            
            // Actualiza las propiedades
            propuestaTorneo.FechaPropuesta = fechaPropuesta;
            propuestaTorneo.Estado = estado;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.Modify()
            _propuestaTorneoRepository.Modify(propuestaTorneo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una propuesta de torneo por su ID.
        /// ADVERTENCIA: Esto puede eliminar también votos y notificaciones asociadas si está configurado en cascade.
        /// </summary>
        /// <param name="idPropuesta">ID de la propuesta a eliminar</param>
        public void Eliminar(long idPropuesta)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.Destroy()
            _propuestaTorneoRepository.Destroy(idPropuesta);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una propuesta de torneo por su identificador único.
        /// </summary>
        /// <param name="idPropuesta">ID de la propuesta</param>
        /// <returns>Entidad PropuestaTorneo o null si no existe</returns>
        public PropuestaTorneo DamePorOID(long idPropuesta)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.DamePorOID()
            return _propuestaTorneoRepository.DamePorOID(idPropuesta);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las propuestas de torneo del sistema.
        /// </summary>
        /// <returns>Lista de todas las propuestas de torneo</returns>
        public IList<PropuestaTorneo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → GenericRepository.DameTodos()
            return _propuestaTorneoRepository.DameTodos();
        }
    }
}
