using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad ParticipacionTorneo.
    /// Expone operaciones CRUD para gestionar la participación de equipos en torneos.
    /// Representa la inscripción formal de un equipo en un torneo después de aprobar la propuesta.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class ParticipacionTorneoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IParticipacionTorneoRepository _participacionTorneoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="participacionTorneoRepository">Implementación del repositorio de participaciones en torneos</param>
        public ParticipacionTorneoCEN(IParticipacionTorneoRepository participacionTorneoRepository)
        {
            _participacionTorneoRepository = participacionTorneoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva participación de equipo en torneo.
        /// REGLA DE NEGOCIO: FechaAlta se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: Estado inicial suele ser ACEPTADA (ya que viene de una propuesta aprobada).
        /// </summary>
        /// <param name="estado">Estado de la participación (ACEPTADA, PENDIENTE, RECHAZADA, RETIRADA)</param>
        /// <returns>ID de la participación creada</returns>
        public long Crear(EstadoParticipacion estado)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var participacionTorneo = new ParticipacionTorneo
            {
                Estado = estado,
                FechaAlta = DateTime.Now  // ← REGLA: Siempre fecha actual
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.New()
            _participacionTorneoRepository.New(participacionTorneo);
            
            return participacionTorneo.IdParticipacion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una participación de torneo existente.
        /// Se usa para cambiar el estado (ej: de ACEPTADA a RETIRADA).
        /// </summary>
        /// <param name="idParticipacion">ID de la participación a modificar</param>
        /// <param name="estado">Nuevo estado de la participación</param>
        /// <param name="fechaAlta">Fecha de alta en el torneo</param>
        public void Modificar(long idParticipacion, EstadoParticipacion estado, DateTime fechaAlta)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.DamePorOID()
            var participacionTorneo = _participacionTorneoRepository.DamePorOID(idParticipacion);
            
            // Actualiza las propiedades
            participacionTorneo.Estado = estado;
            participacionTorneo.FechaAlta = fechaAlta;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.Modify()
            _participacionTorneoRepository.Modify(participacionTorneo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una participación de torneo por su ID.
        /// </summary>
        /// <param name="idParticipacion">ID de la participación a eliminar</param>
        public void Eliminar(long idParticipacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.Destroy()
            _participacionTorneoRepository.Destroy(idParticipacion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una participación de torneo por su identificador único.
        /// </summary>
        /// <param name="idParticipacion">ID de la participación</param>
        /// <returns>Entidad ParticipacionTorneo o null si no existe</returns>
        public ParticipacionTorneo DamePorOID(long idParticipacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.DamePorOID()
            return _participacionTorneoRepository.DamePorOID(idParticipacion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las participaciones de torneos del sistema.
        /// </summary>
        /// <returns>Lista de todas las participaciones</returns>
        public IList<ParticipacionTorneo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ParticipacionTorneoRepository.cs → GenericRepository.DameTodos()
            return _participacionTorneoRepository.DameTodos();
        }
    }
}
