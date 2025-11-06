using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad MiembroEquipo.
    /// Expone operaciones CRUD y métodos custom para gestionar membresías en equipos.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class MiembroEquipoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IMiembroEquipoRepository _repository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="repository">Implementación del repositorio de miembros de equipo</param>
        public MiembroEquipoCEN(IMiembroEquipoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva membresía de equipo.
        /// REGLA DE NEGOCIO: FechaAlta se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: FechaBaja se establece en null (activa).
        /// </summary>
        /// <param name="rol">Rol en el equipo (CAPITAN, MIEMBRO)</param>
        /// <param name="estado">Estado de la membresía (ACTIVA, INACTIVA, EXPULSADA)</param>
        /// <returns>ID del miembro creado</returns>
        public long Crear(RolEquipo rol, EstadoMembresia estado)
        {
            var miembro = new MiembroEquipo
            {
                Rol = rol,
                Estado = estado,
                FechaAlta = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaBaja = null           // ← REGLA: null = membresía activa
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.New()
            _repository.New(miembro);
            return miembro.IdMiembroEquipo;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una membresía de equipo existente.
        /// </summary>
        /// <param name="id">ID del miembro a modificar</param>
        /// <param name="rol">Nuevo rol</param>
        /// <param name="estado">Nuevo estado</param>
        /// <param name="fechaAlta">Nueva fecha de alta</param>
        /// <param name="fechaBaja">Nueva fecha de baja (opcional)</param>
        public void Modificar(long id, RolEquipo rol, EstadoMembresia estado, DateTime fechaAlta, DateTime? fechaBaja = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.DamePorOID()
            var miembro = _repository.DamePorOID(id);
            miembro.Rol = rol;
            miembro.Estado = estado;
            miembro.FechaAlta = fechaAlta;
            miembro.FechaBaja = fechaBaja;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.Modify()
            _repository.Modify(miembro);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una membresía de equipo por su ID.
        /// </summary>
        /// <param name="id">ID del miembro a eliminar</param>
        public void Eliminar(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.Destroy()
            _repository.Destroy(id);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un miembro de equipo por su identificador único.
        /// </summary>
        /// <param name="id">ID del miembro</param>
        /// <returns>Entidad MiembroEquipo o null si no existe</returns>
        public MiembroEquipo DamePorOID(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.DamePorOID()
            return _repository.DamePorOID(id);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los miembros de equipo del sistema.
        /// </summary>
        /// <returns>Lista de todos los miembros de equipo</returns>
        public IList<MiembroEquipo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.DameTodos()
            return _repository.DameTodos();
        }

        /// <summary>
        /// [CUSTOM METHOD] Banea (expulsa) un miembro del equipo.
        /// Cambia el estado a EXPULSADA y establece FechaBaja.
        /// </summary>
        /// <param name="id">ID del miembro a banear</param>
        public void BanearMiembro(long id)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.DamePorOID()
            var miembro = _repository.DamePorOID(id);
            
            // REGLA: Cambiar estado a EXPULSADA y establecer fecha de baja
            miembro.Estado = EstadoMembresia.EXPULSADA;
            miembro.FechaBaja = DateTime.Now;
            
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MiembroEquipoRepository.cs → GenericRepository.Modify()
            _repository.Modify(miembro);
        }
    }
}
