using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Publicacion.
    /// Expone operaciones CRUD y filtros para gestionar publicaciones en comunidades.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class PublicacionCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IPublicacionRepository _publicacionRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="publicacionRepository">Implementación del repositorio de publicaciones</param>
        public PublicacionCEN(IPublicacionRepository publicacionRepository)
        {
            _publicacionRepository = publicacionRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva publicación.
        /// </summary>
        /// <param name="contenido">Contenido de la publicación (obligatorio)</param>
        /// <param name="fechaCreacion">Fecha de creación</param>
        /// <param name="fechaEdicion">Fecha de edición (opcional)</param>
        /// <returns>ID de la publicación creada</returns>
        public long Crear(string contenido, DateTime fechaCreacion, DateTime? fechaEdicion = null)
        {
            var publicacion = new Publicacion
            {
                Contenido = contenido,
                FechaCreacion = fechaCreacion,
                FechaEdicion = fechaEdicion
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.New()
            _publicacionRepository.New(publicacion);
            return publicacion.IdPublicacion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una publicación existente.
        /// </summary>
        /// <param name="idPublicacion">ID de la publicación a modificar</param>
        /// <param name="contenido">Nuevo contenido</param>
        /// <param name="fechaCreacion">Nueva fecha de creación</param>
        /// <param name="fechaEdicion">Nueva fecha de edición (opcional)</param>
        public void Modificar(long idPublicacion, string contenido, DateTime fechaCreacion, DateTime? fechaEdicion = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.DamePorOID()
            var publicacion = _publicacionRepository.DamePorOID(idPublicacion);
            publicacion.Contenido = contenido;
            publicacion.FechaCreacion = fechaCreacion;
            publicacion.FechaEdicion = fechaEdicion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.Modify()
            _publicacionRepository.Modify(publicacion);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una publicación por su ID.
        /// </summary>
        /// <param name="idPublicacion">ID de la publicación a eliminar</param>
        public void Eliminar(long idPublicacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.Destroy()
            _publicacionRepository.Destroy(idPublicacion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una publicación por su identificador único.
        /// </summary>
        /// <param name="idPublicacion">ID de la publicación</param>
        /// <returns>Entidad Publicacion o null si no existe</returns>
        public Publicacion DamePorOID(long idPublicacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.DamePorOID()
            return _publicacionRepository.DamePorOID(idPublicacion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las publicaciones del sistema.
        /// </summary>
        /// <returns>Lista de todas las publicaciones</returns>
        public IList<Publicacion> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → GenericRepository.DameTodos()
            return _publicacionRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra publicaciones por contenido.
        /// </summary>
        /// <param name="filtro">Texto a buscar en el contenido</param>
        /// <returns>Lista de publicaciones filtradas</returns>
        public IList<Publicacion> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PublicacionRepository.cs → DamePorFiltro()
            return _publicacionRepository.DamePorFiltro(filtro);
        }
    }
}
