using System;
using System.Linq;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// CP (Caso de Proceso / Use Case): Aprobación de una propuesta de torneo mediante votación unánime.
    /// 
    /// Este CP implementa LÓGICA DE NEGOCIO COMPLEJA:
    /// 1. Validar que la propuesta existe y está PENDIENTE
    /// 2. Verificar que todos los votos son POSITIVOS (unanimidad requerida)
    /// 3. Si son unánimes: Aprobar propuesta + Crear participación del equipo en el torneo
    /// 4. Confirmar TODAS las operaciones en UNA SOLA TRANSACCIÓN
    /// 
    /// REGLA DE NEGOCIO CRÍTICA: Se requiere UNANIMIDAD (todos los votos deben ser true).
    /// - Si NO hay unanimidad → NO se aprueba (retorna false, NO se hace transacción)
    /// - Si SÍ hay unanimidad → SE aprueba (retorna true, transacción confirmada)
    /// 
    /// Si falla cualquier paso después de la validación → ROLLBACK completo.
    /// </summary>
    public class AprobarPropuestaTorneoCP
    {
        // Dependencias: Repositories para leer/modificar propuesta y crear participación + UnitOfWork para transaccionalidad
        private readonly IPropuestaTorneoRepository _propuestaRepository;
        private readonly IParticipacionTorneoRepository _participacionRepository;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// 
        /// NOTA: Usamos Repositories directamente porque:
        /// - La lógica de votación es ESPECÍFICA de este CP
        /// - No tiene sentido crear CENs que solo harían CRUD básico
        /// - Es más eficiente operar directamente sobre los repositories
        /// 
        /// IMPORTANTE: Todos deben compartir la MISMA instancia de ISession (configurar como Scoped en DI).
        /// </summary>
        /// <param name="propuestaRepository">Repository para operaciones de PropuestaTorneo</param>
        /// <param name="participacionRepository">Repository para operaciones de ParticipacionTorneo</param>
        /// <param name="unitOfWork">UnitOfWork para gestión transaccional</param>
        public AprobarPropuestaTorneoCP(
            IPropuestaTorneoRepository propuestaRepository,
            IParticipacionTorneoRepository participacionRepository,
            IUnitOfWork unitOfWork)
        {
            _propuestaRepository = propuestaRepository;
            _participacionRepository = participacionRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Intenta aprobar una propuesta de torneo verificando la UNANIMIDAD de votos.
        /// 
        /// FLUJO COMPLETO:
        /// 1. FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → DamePorOID()
        ///    - Ejecuta: SELECT * FROM PropuestaTorneo WHERE IdPropuestaTorneo = {idPropuesta}
        ///    - NHibernate CARGA AUTOMÁTICAMENTE la colección Votos (relación 1:N con VotoPropuesta)
        ///    - Retorna la entidad PropuestaTorneo con sus Votos cargados
        /// 
        /// 2. VALIDACIONES (en este CP):
        ///    a) Propuesta existe
        ///    b) Propuesta está en Estado=PENDIENTE (no ya ACEPTADA o RECHAZADA)
        ///    Si alguna falla → InvalidOperationException (transacción no se inicia)
        /// 
        /// 3. LÓGICA DE VOTACIÓN (en este CP):
        ///    a) Convertir colección de Votos a lista: propuesta.Votos.ToList()
        ///    b) Si NO hay votos → Retorna false (no se puede aprobar sin votos)
        ///    c) Contar votos positivos: todosLosVotos.Count(v => v.Valor == true)
        ///    d) Verificar unanimidad: votosPositivos == totalVotos
        ///    e) Si NO son unánimes → Retorna false (no se aprueba, NO se hace transacción)
        /// 
        /// 4. MODIFICACIÓN de la propuesta (solo si hay unanimidad):
        ///    - Cambia Estado → ACEPTADA
        ///    - Marca para UPDATE en BD (no ejecuta SQL todavía)
        /// 
        /// 5. CREACIÓN de la participación (solo si hay unanimidad):
        ///    - Crea nueva entidad ParticipacionTorneo
        ///    - Estado = ACEPTADA (el equipo está aceptado en el torneo)
        ///    - FechaAlta = DateTime.Now (fecha de incorporación al torneo)
        ///    - Establece relaciones: Equipo y Torneo (desde la propuesta)
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 6. FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30) → SaveChanges()
        ///    - AQUÍ se ejecutan TODOS los SQL pendientes
        ///    - Ejecuta: UPDATE PropuestaTorneo SET Estado='ACEPTADA' WHERE IdPropuestaTorneo=...
        ///    - Ejecuta: INSERT INTO ParticipacionTorneo (Estado, FechaAlta, IdEquipo, IdTorneo) VALUES (...)
        ///    - Se hace COMMIT de la transacción
        ///    - Si algo falla, se hace ROLLBACK automático (propuesta NO cambia, participación NO se crea)
        /// 
        /// EJEMPLO DE USO:
        /// // Caso 1: Todos votaron a favor (unanimidad)
        /// var aprobada = aprobarPropuestaTorneoCP.Ejecutar(idPropuesta: 1);
        /// // aprobada = true
        /// // PropuestaTorneo está ACEPTADA
        /// // ParticipacionTorneo creada
        /// 
        /// // Caso 2: Hay al menos un voto en contra (no unanimidad)
        /// var aprobada = aprobarPropuestaTorneoCP.Ejecutar(idPropuesta: 2);
        /// // aprobada = false
        /// // PropuestaTorneo sigue PENDIENTE
        /// // ParticipacionTorneo NO se crea
        /// </summary>
        /// <param name="idPropuesta">ID de la propuesta de torneo a evaluar</param>
        /// <returns>
        /// true: La propuesta tenía unanimidad y fue APROBADA (transacción confirmada)
        /// false: No había votos o no había unanimidad (NO se aprueba, NO se hace transacción)
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// - Si la propuesta no existe
        /// - Si la propuesta ya fue resuelta (no está PENDIENTE)
        /// </exception>
        /// <exception cref="Exception">
        /// Si cualquier operación de BD falla después de validar unanimidad,
        /// se propaga la excepción y se hace ROLLBACK automático.
        /// </exception>
        public bool Ejecutar(long idPropuesta)
        {
            // PASO 1: Obtener la propuesta con sus votos
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PropuestaTorneoRepository.cs → DamePorOID()
            // Ejecuta: SELECT * FROM PropuestaTorneo WHERE IdPropuestaTorneo = {idPropuesta}
            // NHibernate CARGA la colección Votos automáticamente (relación 1:N)
            var propuesta = _propuestaRepository.DamePorOID(idPropuesta);

            // VALIDACIÓN 1: La propuesta debe existir
            if (propuesta == null)
            {
                throw new InvalidOperationException("La propuesta no existe.");
            }

            // VALIDACIÓN 2: Debe estar PENDIENTE (no ya ACEPTADA o RECHAZADA)
            // Estados posibles: EstadoSolicitud.PENDIENTE, EstadoSolicitud.ACEPTADA, EstadoSolicitud.RECHAZADA
            // REGLA DE NEGOCIO: Solo se puede aprobar UNA VEZ
            if (propuesta.Estado != EstadoSolicitud.PENDIENTE)
            {
                throw new InvalidOperationException("La propuesta ya fue resuelta.");
            }

            // PASO 2: Verificar si los votos son UNÁNIMES (REGLA DE NEGOCIO CRÍTICA)
            // Convertir la colección de Votos a lista para LINQ
            var todosLosVotos = propuesta.Votos.ToList();
            
            // VALIDACIÓN 3: Debe haber al menos un voto
            if (todosLosVotos.Count == 0)
            {
                return false; // No hay votos, no se puede aprobar (NO se hace transacción)
            }

            // Contar cuántos votos son POSITIVOS (Valor = true)
            // VotoPropuesta tiene propiedad bool Valor: true=A_FAVOR, false=EN_CONTRA
            var votosPositivos = todosLosVotos.Count(v => v.Valor);
            
            // VERIFICAR UNANIMIDAD: Todos los votos deben ser positivos
            var sonUnanimes = votosPositivos == todosLosVotos.Count;

            // Si NO hay unanimidad → NO se aprueba (no se hace transacción)
            if (!sonUnanimes)
            {
                return false; // Hay al menos un voto en contra (NO se hace transacción)
            }

            // A partir de aquí: HAY UNANIMIDAD → Proceder a aprobar

            // PASO 3: Aprobar la propuesta
            // Cambiar de PENDIENTE → ACEPTADA
            propuesta.Estado = EstadoSolicitud.ACEPTADA;
            // Marcar para UPDATE (no ejecuta SQL todavía)
            _propuestaRepository.Modify(propuesta);

            // PASO 4: Crear participación en el torneo
            // IMPORTANTE: Usamos "new EN.ParticipacionTorneo" directamente porque:
            // - ParticipacionTorneoCEN (si existiera) solo tendría Crear() sin lógica adicional
            // - Es más simple crear la entidad directamente aquí
            var participacion = new EN.ParticipacionTorneo
            {
                Estado = EstadoParticipacion.ACEPTADA,  // El equipo está ACEPTADO en el torneo
                FechaAlta = DateTime.Now,               // Fecha de incorporación al torneo
                Equipo = propuesta.Equipo,              // Relación: el equipo que propuso participar
                Torneo = propuesta.Torneo               // Relación: el torneo al que se postuló
            };
            // Marcar para INSERT (no ejecuta SQL todavía)
            _participacionRepository.New(participacion);

            // PASO 5: COMMIT de la transacción - AQUÍ se ejecutan TODOS los SQL
            // FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
            // Ejecuta:
            // - UPDATE PropuestaTorneo SET Estado='ACEPTADA' WHERE IdPropuestaTorneo={idPropuesta}
            // - INSERT INTO ParticipacionTorneo (Estado, FechaAlta, IdEquipo, IdTorneo) VALUES (...)
            // - COMMIT (confirma ambos cambios)
            // Si cualquiera falla → ROLLBACK (propuesta NO cambia, participación NO se crea)
            _unitOfWork.SaveChanges();

            // Retornar true: La propuesta fue APROBADA exitosamente
            return true;
        }
    }
}
