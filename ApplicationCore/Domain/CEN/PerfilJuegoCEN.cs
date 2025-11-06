using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad PerfilJuego.
    /// Expone operaciones CRUD para gestionar la relación entre perfiles y juegos.
    /// Representa los juegos que un usuario tiene en su colección/lista.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class PerfilJuegoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IPerfilJuegoRepository _perfilJuegoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="perfilJuegoRepository">Implementación del repositorio de perfil-juego</param>
        public PerfilJuegoCEN(IPerfilJuegoRepository perfilJuegoRepository)
        {
            _perfilJuegoRepository = perfilJuegoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva relación entre perfil y juego.
        /// REGLA DE NEGOCIO: FechaAdicion se establece automáticamente a DateTime.Now.
        /// </summary>
        /// <returns>ID de la relación creada</returns>
        public long Crear()
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var perfilJuego = new PerfilJuego
            {
                FechaAdicion = DateTime.Now  // ← REGLA: Siempre fecha actual
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.New()
            _perfilJuegoRepository.New(perfilJuego);
            
            return perfilJuego.IdPerfilJuego;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una relación perfil-juego existente.
        /// Normalmente esta entidad no se modifica, pero se incluye para completitud.
        /// </summary>
        /// <param name="idPerfilJuego">ID de la relación a modificar</param>
        /// <param name="fechaAdicion">Fecha de adición del juego al perfil</param>
        public void Modificar(long idPerfilJuego, DateTime fechaAdicion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.DamePorOID()
            var perfilJuego = _perfilJuegoRepository.DamePorOID(idPerfilJuego);
            
            // Actualiza las propiedades
            perfilJuego.FechaAdicion = fechaAdicion;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.Modify()
            _perfilJuegoRepository.Modify(perfilJuego);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una relación perfil-juego por su ID.
        /// Se usa cuando un usuario quiere quitar un juego de su lista.
        /// </summary>
        /// <param name="idPerfilJuego">ID de la relación a eliminar</param>
        public void Eliminar(long idPerfilJuego)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.Destroy()
            _perfilJuegoRepository.Destroy(idPerfilJuego);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una relación perfil-juego por su identificador único.
        /// </summary>
        /// <param name="idPerfilJuego">ID de la relación</param>
        /// <returns>Entidad PerfilJuego o null si no existe</returns>
        public PerfilJuego DamePorOID(long idPerfilJuego)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.DamePorOID()
            return _perfilJuegoRepository.DamePorOID(idPerfilJuego);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las relaciones perfil-juego del sistema.
        /// </summary>
        /// <returns>Lista de todas las relaciones perfil-juego</returns>
        public IList<PerfilJuego> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilJuegoRepository.cs → GenericRepository.DameTodos()
            return _perfilJuegoRepository.DameTodos();
        }
    }
}
