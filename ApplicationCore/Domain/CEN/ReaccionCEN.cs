using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Reaccion.
    /// Expone operaciones CRUD para gestionar reacciones a publicaciones y comentarios.
    /// REGLA IMPORTANTE: Una reacción por usuario y tipo por objetivo (publicación o comentario).
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class ReaccionCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IReaccionRepository _reaccionRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="reaccionRepository">Implementación del repositorio de reacciones</param>
        public ReaccionCEN(IReaccionRepository reaccionRepository)
        {
            _reaccionRepository = reaccionRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva reacción.
        /// REGLA DE NEGOCIO: FechaCreacion se establece automáticamente a DateTime.Now.
        /// RESTRICCION: Solo una reacción por usuario y tipo por objetivo.
        /// </summary>
        /// <param name="tipo">Tipo de reacción (ME_GUSTA, OTRO, etc.)</param>
        /// <returns>ID de la reacción creada</returns>
        public long Crear(TipoReaccion tipo)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var reaccion = new Reaccion
            {
                Tipo = tipo,
                FechaCreacion = DateTime.Now  // ← REGLA: Siempre fecha actual
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.New()
            _reaccionRepository.New(reaccion);
            
            return reaccion.IdReaccion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una reacción existente.
        /// Permite cambiar el tipo de reacción (ME_GUSTA → OTRO, etc.).
        /// </summary>
        /// <param name="idReaccion">ID de la reacción a modificar</param>
        /// <param name="tipo">Nuevo tipo de reacción</param>
        /// <param name="fechaCreacion">Fecha de creación original</param>
        public void Modificar(long idReaccion, TipoReaccion tipo, DateTime fechaCreacion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.DamePorOID()
            var reaccion = _reaccionRepository.DamePorOID(idReaccion);
            
            // Actualiza las propiedades
            reaccion.Tipo = tipo;
            reaccion.FechaCreacion = fechaCreacion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.Modify()
            _reaccionRepository.Modify(reaccion);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una reacción por su ID.
        /// Se usa cuando un usuario quiere quitar su reacción.
        /// </summary>
        /// <param name="idReaccion">ID de la reacción a eliminar</param>
        public void Eliminar(long idReaccion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.Destroy()
            _reaccionRepository.Destroy(idReaccion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una reacción por su identificador único.
        /// </summary>
        /// <param name="idReaccion">ID de la reacción</param>
        /// <returns>Entidad Reaccion o null si no existe</returns>
        public Reaccion DamePorOID(long idReaccion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.DamePorOID()
            return _reaccionRepository.DamePorOID(idReaccion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las reacciones del sistema.
        /// </summary>
        /// <returns>Lista de todas las reacciones</returns>
        public IList<Reaccion> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ReaccionRepository.cs → GenericRepository.DameTodos()
            return _reaccionRepository.DameTodos();
        }
    }
}
