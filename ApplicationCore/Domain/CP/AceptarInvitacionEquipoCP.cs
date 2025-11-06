using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// CP (Caso de Proceso / Use Case): Aceptación de una invitación de equipo.
    /// 
    /// Este CP coordina múltiples operaciones:
    /// 1. Validar que la invitación existe, es de tipo EQUIPO y está PENDIENTE
    /// 2. Actualizar el estado de la invitación a ACEPTADA
    /// 3. Crear la membresía del usuario en el equipo
    /// 4. Confirmar TODAS las operaciones en UNA SOLA TRANSACCIÓN
    /// 
    /// REGLA DE NEGOCIO: Una invitación solo puede aceptarse UNA VEZ (estado PENDIENTE).
    /// Si falla cualquier paso → ROLLBACK completo (invitación no cambia, membresía no se crea).
    /// </summary>
    public class AceptarInvitacionEquipoCP
    {
        // Dependencias: Repository para leer/modificar invitación + CEN para crear membresía + UnitOfWork para transaccionalidad
        private readonly IInvitacionRepository _invitacionRepository;
        private readonly MiembroEquipoCEN _miembroEquipoCEN;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// 
        /// NOTA: Aquí mezclamos Repository + CEN porque:
        /// - InvitacionCEN (si existiera) solo tendría CRUD básico
        /// - La LÓGICA DE VALIDACIÓN específica está en este CP
        /// - Es más eficiente usar el Repository directamente
        /// 
        /// IMPORTANTE: Todos deben compartir la MISMA instancia de ISession (configurar como Scoped en DI).
        /// </summary>
        /// <param name="invitacionRepository">Repository para operaciones de Invitacion</param>
        /// <param name="miembroEquipoCEN">CEN para operaciones de MiembroEquipo</param>
        /// <param name="unitOfWork">UnitOfWork para gestión transaccional</param>
        public AceptarInvitacionEquipoCP(
            IInvitacionRepository invitacionRepository,
            MiembroEquipoCEN miembroEquipoCEN,
            IUnitOfWork unitOfWork)
        {
            _invitacionRepository = invitacionRepository;
            _miembroEquipoCEN = miembroEquipoCEN;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Acepta una invitación de equipo y crea la membresía correspondiente de forma TRANSACCIONAL.
        /// 
        /// FLUJO COMPLETO:
        /// 1. FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → DamePorOID()
        ///    - Ejecuta: SELECT * FROM Invitacion WHERE IdInvitacion = {idInvitacion}
        ///    - Retorna la entidad Invitacion o null
        /// 
        /// 2. VALIDACIONES (en este CP):
        ///    a) Invitación existe
        ///    b) Invitación es de Tipo=EQUIPO (no COMUNIDAD, no TORNEO)
        ///    c) Invitación está en Estado=PENDIENTE (no ya ACEPTADA o RECHAZADA)
        ///    Si alguna falla → InvalidOperationException (transacción no se inicia)
        /// 
        /// 3. MODIFICACIÓN de la invitación (en este CP):
        ///    - Cambia Estado → ACEPTADA
        ///    - Establece FechaRespuesta → DateTime.Now
        ///    - Marca para UPDATE en BD (no ejecuta SQL todavía)
        /// 
        /// 4. FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroEquipoCEN.cs → Crear()
        ///    - Crea entidad MiembroEquipo con Rol=MIEMBRO, Estado=ACTIVA
        ///    - REGLA DE NEGOCIO: FechaBaja=null, FechaAlta=DateTime.Now (aplicada automáticamente)
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 5. FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30) → SaveChanges()
        ///    - AQUÍ se ejecutan TODOS los SQL pendientes
        ///    - Ejecuta: UPDATE Invitacion SET Estado='ACEPTADA', FechaRespuesta=... WHERE IdInvitacion=...
        ///    - Ejecuta: INSERT INTO MiembroEquipo (...)
        ///    - Se hace COMMIT de la transacción
        ///    - Si algo falla, se hace ROLLBACK automático (invitación NO cambia, membresía NO se crea)
        /// 
        /// EJEMPLO DE USO:
        /// var idMiembro = aceptarInvitacionEquipoCP.Ejecutar(idInvitacion: 1);
        /// // idMiembro contiene el ID de la nueva membresía creada
        /// // La invitación está marcada como ACEPTADA
        /// // El usuario ya es MIEMBRO del equipo
        /// </summary>
        /// <param name="idInvitacion">ID de la invitación de equipo a aceptar</param>
        /// <returns>ID del nuevo MiembroEquipo creado</returns>
        /// <exception cref="InvalidOperationException">
        /// - Si la invitación no existe
        /// - Si la invitación no es de tipo EQUIPO
        /// - Si la invitación ya fue respondida (no está PENDIENTE)
        /// </exception>
        /// <exception cref="Exception">
        /// Si cualquier operación de BD falla, se propaga la excepción y se hace ROLLBACK automático.
        /// </exception>
        public long Ejecutar(long idInvitacion)
        {
            // PASO 1: Obtener la invitación
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/InvitacionRepository.cs → DamePorOID()
            // Ejecuta: SELECT * FROM Invitacion WHERE IdInvitacion = {idInvitacion}
            var invitacion = _invitacionRepository.DamePorOID(idInvitacion);

            // VALIDACIÓN 1: La invitación debe existir
            if (invitacion == null)
            {
                throw new InvalidOperationException("La invitación no existe.");
            }

            // VALIDACIÓN 2: Debe ser invitación de EQUIPO (no COMUNIDAD, no TORNEO)
            // Tipos posibles: TipoInvitacion.EQUIPO, TipoInvitacion.COMUNIDAD, TipoInvitacion.TORNEO
            if (invitacion.Tipo != TipoInvitacion.EQUIPO)
            {
                throw new InvalidOperationException("La invitación no es de tipo equipo.");
            }

            // VALIDACIÓN 3: Debe estar PENDIENTE (no ya ACEPTADA o RECHAZADA)
            // Estados posibles: EstadoSolicitud.PENDIENTE, EstadoSolicitud.ACEPTADA, EstadoSolicitud.RECHAZADA
            // REGLA DE NEGOCIO: Solo se puede aceptar UNA VEZ
            if (invitacion.Estado != EstadoSolicitud.PENDIENTE)
            {
                throw new InvalidOperationException("La invitación ya fue respondida.");
            }

            // PASO 2: Actualizar estado de la invitación
            // Cambiar de PENDIENTE → ACEPTADA
            invitacion.Estado = EstadoSolicitud.ACEPTADA;
            // Registrar fecha/hora de respuesta (momento actual del servidor)
            invitacion.FechaRespuesta = DateTime.Now;
            // Marcar para UPDATE (no ejecuta SQL todavía)
            _invitacionRepository.Modify(invitacion);

            // PASO 3: Crear membresía de equipo
            // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroEquipoCEN.cs → Crear()
            // REGLA DE NEGOCIO: Rol por defecto es MIEMBRO (no LIDER, no MODERADOR)
            var idMiembro = _miembroEquipoCEN.Crear(
                rol: RolEquipo.MIEMBRO,        // Rol inicial: MIEMBRO
                estado: EstadoMembresia.ACTIVA // Estado: ACTIVA desde el inicio
            );
            // En este punto: MiembroEquipo está marcado para INSERT pero NO está en BD todavía

            // PASO 4: COMMIT de la transacción - AQUÍ se ejecutan TODOS los SQL
            // FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
            // Ejecuta:
            // - UPDATE Invitacion SET Estado='ACEPTADA', FechaRespuesta=... WHERE IdInvitacion={idInvitacion}
            // - INSERT INTO MiembroEquipo (...)
            // - COMMIT (confirma ambos cambios)
            // Si cualquiera falla → ROLLBACK (invitación NO cambia, membresía NO se crea)
            _unitOfWork.SaveChanges();

            // Retorna el ID de la membresía creada (disponible inmediatamente gracias a HiLo)
            return idMiembro;
        }
    }
}
