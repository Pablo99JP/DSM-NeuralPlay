using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Comentario.
    /// Expone operaciones CRUD para gestionar comentarios en publicaciones.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class ComentarioCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IComentarioRepository _comentarioRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="comentarioRepository">Implementación del repositorio de comentarios</param>
        public ComentarioCEN(IComentarioRepository comentarioRepository)
        {
            _comentarioRepository = comentarioRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo comentario en una publicación.
        /// REGLA DE NEGOCIO: FechaCreacion se establece automáticamente a DateTime.Now.
        /// </summary>
        /// <param name="contenido">Contenido del comentario (obligatorio)</param>
        /// <param name="fechaEdicion">Fecha de edición (opcional, null si no se ha editado)</param>
        /// <returns>ID del comentario creado</returns>
        public long Crear(string contenido, DateTime? fechaEdicion = null)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var comentario = new Comentario
            {
                Contenido = contenido,
                FechaCreacion = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaEdicion = fechaEdicion
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.New()
            _comentarioRepository.New(comentario);
            
            return comentario.IdComentario;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un comentario existente.
        /// Actualiza FechaEdicion automáticamente al modificar el contenido.
        /// </summary>
        /// <param name="idComentario">ID del comentario a modificar</param>
        /// <param name="contenido">Nuevo contenido del comentario</param>
        /// <param name="fechaCreacion">Fecha de creación original (normalmente no se modifica)</param>
        /// <param name="fechaEdicion">Fecha de edición (opcional)</param>
        public void Modificar(long idComentario, string contenido, DateTime fechaCreacion, DateTime? fechaEdicion = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.DamePorOID()
            var comentario = _comentarioRepository.DamePorOID(idComentario);
            
            // Actualiza las propiedades
            comentario.Contenido = contenido;
            comentario.FechaCreacion = fechaCreacion;
            comentario.FechaEdicion = fechaEdicion ?? DateTime.Now;  // ← REGLA: Actualizar fecha de edición

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.Modify()
            _comentarioRepository.Modify(comentario);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un comentario por su ID.
        /// ADVERTENCIA: Esto puede eliminar también reacciones y notificaciones asociadas si está configurado en cascade.
        /// </summary>
        /// <param name="idComentario">ID del comentario a eliminar</param>
        public void Eliminar(long idComentario)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.Destroy()
            _comentarioRepository.Destroy(idComentario);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un comentario por su identificador único.
        /// </summary>
        /// <param name="idComentario">ID del comentario</param>
        /// <returns>Entidad Comentario o null si no existe</returns>
        public Comentario DamePorOID(long idComentario)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.DamePorOID()
            return _comentarioRepository.DamePorOID(idComentario);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los comentarios del sistema.
        /// </summary>
        /// <returns>Lista de todos los comentarios</returns>
        public IList<Comentario> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ComentarioRepository.cs → GenericRepository.DameTodos()
            return _comentarioRepository.DameTodos();
        }
    }
}
