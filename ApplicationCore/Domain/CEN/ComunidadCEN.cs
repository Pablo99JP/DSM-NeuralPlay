using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Comunidad.
    /// Expone operaciones CRUD y filtros para gestionar comunidades de jugadores.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class ComunidadCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IComunidadRepository _comunidadRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="comunidadRepository">Implementación del repositorio de comunidades</param>
        public ComunidadCEN(IComunidadRepository comunidadRepository)
        {
            _comunidadRepository = comunidadRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva comunidad.
        /// </summary>
        /// <param name="nombre">Nombre de la comunidad (obligatorio)</param>
        /// <param name="fechaCreacion">Fecha de creación de la comunidad</param>
        /// <param name="descripcion">Descripción de la comunidad (opcional)</param>
        /// <returns>ID de la comunidad creada</returns>
        public long Crear(string nombre, DateTime fechaCreacion, string descripcion = null)
        {
            var comunidad = new Comunidad
            {
                Nombre = nombre,
                FechaCreacion = fechaCreacion,
                Descripcion = descripcion
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.New()
            _comunidadRepository.New(comunidad);
            return comunidad.IdComunidad;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una comunidad existente.
        /// </summary>
        /// <param name="idComunidad">ID de la comunidad a modificar</param>
        /// <param name="nombre">Nuevo nombre</param>
        /// <param name="fechaCreacion">Nueva fecha de creación</param>
        /// <param name="descripcion">Nueva descripción (opcional)</param>
        public void Modificar(long idComunidad, string nombre, DateTime fechaCreacion, string descripcion = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.DamePorOID()
            var comunidad = _comunidadRepository.DamePorOID(idComunidad);
            comunidad.Nombre = nombre;
            comunidad.FechaCreacion = fechaCreacion;
            comunidad.Descripcion = descripcion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.Modify()
            _comunidadRepository.Modify(comunidad);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una comunidad por su ID.
        /// </summary>
        /// <param name="idComunidad">ID de la comunidad a eliminar</param>
        public void Eliminar(long idComunidad)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.Destroy()
            _comunidadRepository.Destroy(idComunidad);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una comunidad por su identificador único.
        /// </summary>
        /// <param name="idComunidad">ID de la comunidad</param>
        /// <returns>Entidad Comunidad o null si no existe</returns>
        public Comunidad DamePorOID(long idComunidad)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.DamePorOID()
            return _comunidadRepository.DamePorOID(idComunidad);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las comunidades del sistema.
        /// </summary>
        /// <returns>Lista de todas las comunidades</returns>
        public IList<Comunidad> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → GenericRepository.DameTodos()
            return _comunidadRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra comunidades por nombre o descripción.
        /// </summary>
        /// <param name="filtro">Texto a buscar en nombre o descripción</param>
        /// <returns>Lista de comunidades filtradas</returns>
        public IList<Comunidad> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComunidadRepository.cs → DamePorFiltro()
            return _comunidadRepository.DamePorFiltro(filtro);
        }
    }
}
