using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad MiembroComunidad.
    /// Expone operaciones CRUD y métodos custom para gestionar membresías en comunidades.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class MiembroComunidadCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IMiembroComunidadRepository _repository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="repository">Implementación del repositorio de miembros de comunidad</param>
        public MiembroComunidadCEN(IMiembroComunidadRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva membresía de comunidad.
        /// REGLA DE NEGOCIO: FechaAlta se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: FechaBaja se establece en null (activa).
        /// </summary>
        /// <param name="rol">Rol en la comunidad (LIDER, COLABORADOR, MIEMBRO)</param>
        /// <param name="estado">Estado de la membresía (ACTIVA, INACTIVA, EXPULSADA)</param>
        /// <returns>ID del miembro creado</returns>
        public long Crear(RolComunidad rol, EstadoMembresia estado)
        {
            var miembro = new MiembroComunidad
            {
                Rol = rol,
                Estado = estado,
                FechaAlta = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaBaja = null           // ← REGLA: null = membresía activa
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.New()
            _repository.New(miembro);
            return miembro.IdMiembroComunidad;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una membresía de comunidad existente.
        /// </summary>
        /// <param name="id">ID del miembro a modificar</param>
        /// <param name="rol">Nuevo rol</param>
        /// <param name="estado">Nuevo estado</param>
        /// <param name="fechaAlta">Nueva fecha de alta</param>
        /// <param name="fechaBaja">Nueva fecha de baja (opcional)</param>
        public void Modificar(long id, RolComunidad rol, EstadoMembresia estado, DateTime fechaAlta, DateTime? fechaBaja = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.DamePorOID()
            var miembro = _repository.DamePorOID(id);
            miembro.Rol = rol;
            miembro.Estado = estado;
            miembro.FechaAlta = fechaAlta;
            miembro.FechaBaja = fechaBaja;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.Modify()
            _repository.Modify(miembro);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una membresía de comunidad por su ID.
        /// </summary>
        /// <param name="id">ID del miembro a eliminar</param>
        public void Eliminar(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.Destroy()
            _repository.Destroy(id);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un miembro de comunidad por su identificador único.
        /// </summary>
        /// <param name="id">ID del miembro</param>
        /// <returns>Entidad MiembroComunidad o null si no existe</returns>
        public MiembroComunidad DamePorOID(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.DamePorOID()
            return _repository.DamePorOID(id);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los miembros de comunidad del sistema.
        /// </summary>
        /// <returns>Lista de todos los miembros de comunidad</returns>
        public IList<MiembroComunidad> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.DameTodos()
            return _repository.DameTodos();
        }

        /// <summary>
        /// [CUSTOM METHOD] Promociona un miembro a rol COLABORADOR (moderador).
        /// </summary>
        /// <param name="id">ID del miembro a promocionar</param>
        public void PromoverAModerador(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.DamePorOID()
            var miembro = _repository.DamePorOID(id);
            
            // REGLA: Cambiar rol a COLABORADOR
            miembro.Rol = RolComunidad.COLABORADOR;
            
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.Modify()
            _repository.Modify(miembro);
        }

        /// <summary>
        /// [CUSTOM METHOD] Actualiza la fecha de alta de un miembro.
        /// </summary>
        /// <param name="id">ID del miembro</param>
        /// <param name="nuevaFecha">Nueva fecha de alta</param>
        public void ActualizarFechaAccion(long id, DateTime nuevaFecha)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.DamePorOID()
            var miembro = _repository.DamePorOID(id);
            miembro.FechaAlta = nuevaFecha;
            
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroComunidadRepository.cs → GenericRepository.Modify()
            _repository.Modify(miembro);
        }
    }
}
