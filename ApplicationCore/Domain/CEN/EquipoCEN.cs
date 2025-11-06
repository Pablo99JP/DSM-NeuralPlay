using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Equipo.
    /// Expone operaciones CRUD y filtros para gestionar equipos dentro de comunidades.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class EquipoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IEquipoRepository _equipoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="equipoRepository">Implementación del repositorio de equipos</param>
        public EquipoCEN(IEquipoRepository equipoRepository)
        {
            _equipoRepository = equipoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo equipo.
        /// </summary>
        /// <param name="nombre">Nombre del equipo (obligatorio)</param>
        /// <param name="fechaCreacion">Fecha de creación del equipo</param>
        /// <param name="descripcion">Descripción del equipo (opcional)</param>
        /// <returns>ID del equipo creado</returns>
        public long Crear(string nombre, DateTime fechaCreacion, string descripcion = null)
        {
            var equipo = new Equipo
            {
                Nombre = nombre,
                FechaCreacion = fechaCreacion,
                Descripcion = descripcion
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.New()
            _equipoRepository.New(equipo);
            return equipo.IdEquipo;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un equipo existente.
        /// </summary>
        /// <param name="idEquipo">ID del equipo a modificar</param>
        /// <param name="nombre">Nuevo nombre</param>
        /// <param name="fechaCreacion">Nueva fecha de creación</param>
        /// <param name="descripcion">Nueva descripción (opcional)</param>
        public void Modificar(long idEquipo, string nombre, DateTime fechaCreacion, string descripcion = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.DamePorOID()
            var equipo = _equipoRepository.DamePorOID(idEquipo);
            equipo.Nombre = nombre;
            equipo.FechaCreacion = fechaCreacion;
            equipo.Descripcion = descripcion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.Modify()
            _equipoRepository.Modify(equipo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un equipo por su ID.
        /// </summary>
        /// <param name="idEquipo">ID del equipo a eliminar</param>
        public void Eliminar(long idEquipo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.Destroy()
            _equipoRepository.Destroy(idEquipo);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un equipo por su identificador único.
        /// </summary>
        /// <param name="idEquipo">ID del equipo</param>
        /// <returns>Entidad Equipo o null si no existe</returns>
        public Equipo DamePorOID(long idEquipo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.DamePorOID()
            return _equipoRepository.DamePorOID(idEquipo);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los equipos del sistema.
        /// </summary>
        /// <returns>Lista de todos los equipos</returns>
        public IList<Equipo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → GenericRepository.DameTodos()
            return _equipoRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra equipos por nombre o descripción.
        /// </summary>
        /// <param name="filtro">Texto a buscar en nombre o descripción</param>
        /// <returns>Lista de equipos filtrados</returns>
        public IList<Equipo> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/EquipoRepository.cs → DamePorFiltro()
            return _equipoRepository.DamePorFiltro(filtro);
        }
    }
}
