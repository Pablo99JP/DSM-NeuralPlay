using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad SolicitudIngreso.
    /// Expone operaciones CRUD y métodos custom para gestionar solicitudes de ingreso.
    /// Incluye validaciones de negocio para evitar membresías duplicadas.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class SolicitudIngresoCEN
    {
        // Dependencias: Interfaces de repositorios (NO implementaciones concretas)
        private readonly ISolicitudIngresoRepository _repository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IComunidadRepository _comunidadRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="repository">Implementación del repositorio de solicitudes</param>
        /// <param name="usuarioRepository">Implementación del repositorio de usuarios</param>
        /// <param name="comunidadRepository">Implementación del repositorio de comunidades</param>
        public SolicitudIngresoCEN(
            ISolicitudIngresoRepository repository,
            IUsuarioRepository usuarioRepository,
            IComunidadRepository comunidadRepository)
        {
            _repository = repository;
            _usuarioRepository = usuarioRepository;
            _comunidadRepository = comunidadRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva solicitud de ingreso.
        /// REGLA DE NEGOCIO: FechaSolicitud se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: El usuario no puede estar ya en la comunidad ni en un equipo de esa comunidad.
        /// </summary>
        /// <param name="tipo">Tipo de invitación (COMUNIDAD o EQUIPO)</param>
        /// <param name="estado">Estado inicial (normalmente PENDIENTE)</param>
        /// <param name="idUsuario">ID del usuario solicitante</param>
        /// <param name="idComunidad">ID de la comunidad (opcional)</param>
        /// <param name="idEquipo">ID del equipo (opcional)</param>
        /// <returns>ID de la solicitud creada</returns>
        public long Crear(TipoInvitacion tipo, EstadoSolicitud estado, long idUsuario, long? idComunidad = null, long? idEquipo = null)
        {
            // REGLA: Validación - el usuario no debe estar ya en un equipo de esa comunidad
            if (idComunidad.HasValue)
            {
                var usuario = _usuarioRepository.DamePorOID(idUsuario);
                var comunidad = _comunidadRepository.DamePorOID(idComunidad.Value);
                
                var yaEsMiembro = usuario.MiembrosComunidad
                    .Any(mc => mc.Comunidad.IdComunidad == idComunidad.Value && 
                               mc.Estado == EstadoMembresia.ACTIVA);

                if (yaEsMiembro)
                {
                    throw new InvalidOperationException(
                        "El usuario ya es miembro de esta comunidad.");
                }

                // Verificar si ya está en algún equipo de la comunidad
                var yaEnEquipoComunidad = usuario.MiembrosEquipo
                    .Any(me => me.Equipo.Comunidad.IdComunidad == idComunidad.Value && 
                               me.Estado == EstadoMembresia.ACTIVA);

                if (yaEnEquipoComunidad)
                {
                    throw new InvalidOperationException(
                        "El usuario ya está en un equipo de esta comunidad.");
                }
            }

            var solicitud = new SolicitudIngreso
            {
                Tipo = tipo,
                Estado = estado,
                FechaSolicitud = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaResolucion = null          // ← REGLA: null = pendiente
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.New()
            _repository.New(solicitud);
            return solicitud.IdSolicitud;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una solicitud de ingreso existente.
        /// </summary>
        /// <param name="id">ID de la solicitud a modificar</param>
        /// <param name="tipo">Nuevo tipo</param>
        /// <param name="estado">Nuevo estado</param>
        /// <param name="fechaSolicitud">Nueva fecha de solicitud</param>
        /// <param name="fechaResolucion">Nueva fecha de resolución (opcional)</param>
        public void Modificar(long id, TipoInvitacion tipo, EstadoSolicitud estado, DateTime fechaSolicitud, DateTime? fechaResolucion = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.DamePorOID()
            var solicitud = _repository.DamePorOID(id);
            solicitud.Tipo = tipo;
            solicitud.Estado = estado;
            solicitud.FechaSolicitud = fechaSolicitud;
            solicitud.FechaResolucion = fechaResolucion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.Modify()
            _repository.Modify(solicitud);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una solicitud de ingreso por su ID.
        /// </summary>
        /// <param name="id">ID de la solicitud a eliminar</param>
        public void Eliminar(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.Destroy()
            _repository.Destroy(id);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una solicitud de ingreso por su identificador único.
        /// </summary>
        /// <param name="id">ID de la solicitud</param>
        /// <returns>Entidad SolicitudIngreso o null si no existe</returns>
        public SolicitudIngreso DamePorOID(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.DamePorOID()
            return _repository.DamePorOID(id);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las solicitudes de ingreso del sistema.
        /// </summary>
        /// <returns>Lista de todas las solicitudes</returns>
        public IList<SolicitudIngreso> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.DameTodos()
            return _repository.DameTodos();
        }

        /// <summary>
        /// [CUSTOM METHOD] Aprueba una solicitud de ingreso.
        /// Cambia el estado a ACEPTADA y establece FechaResolucion.
        /// </summary>
        /// <param name="id">ID de la solicitud a aprobar</param>
        public void Aprobar(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.DamePorOID()
            var solicitud = _repository.DamePorOID(id);
            
            // REGLA: Cambiar estado a ACEPTADA y establecer fecha de resolución
            solicitud.Estado = EstadoSolicitud.ACEPTADA;
            solicitud.FechaResolucion = DateTime.Now;
            
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.Modify()
            _repository.Modify(solicitud);
        }

        /// <summary>
        /// [CUSTOM METHOD] Rechaza una solicitud de ingreso.
        /// Cambia el estado a RECHAZADA y establece FechaResolucion.
        /// </summary>
        /// <param name="id">ID de la solicitud a rechazar</param>
        public void Rechazar(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.DamePorOID()
            var solicitud = _repository.DamePorOID(id);
            
            // REGLA: Cambiar estado a RECHAZADA y establecer fecha de resolución
            solicitud.Estado = EstadoSolicitud.RECHAZADA;
            solicitud.FechaResolucion = DateTime.Now;
            
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SolicitudIngresoRepository.cs → GenericRepository.Modify()
            _repository.Modify(solicitud);
        }
    }
}
