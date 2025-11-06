using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Juego.
    /// Expone operaciones CRUD y filtros para gestionar catálogo de juegos.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class JuegoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IJuegoRepository _juegoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="juegoRepository">Implementación del repositorio de juegos</param>
        public JuegoCEN(IJuegoRepository juegoRepository)
        {
            _juegoRepository = juegoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo juego en el catálogo.
        /// </summary>
        /// <param name="nombreJuego">Nombre del juego (obligatorio)</param>
        /// <param name="genero">Género del juego (MOBA, FPS, RPG, etc.)</param>
        /// <returns>ID del juego creado</returns>
        public long Crear(string nombreJuego, GeneroJuego genero)
        {
            var juego = new Juego
            {
                NombreJuego = nombreJuego,
                Genero = genero
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.New()
            _juegoRepository.New(juego);
            return juego.IdJuego;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un juego existente.
        /// </summary>
        /// <param name="idJuego">ID del juego a modificar</param>
        /// <param name="nombreJuego">Nuevo nombre</param>
        /// <param name="genero">Nuevo género</param>
        public void Modificar(long idJuego, string nombreJuego, GeneroJuego genero)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.DamePorOID()
            var juego = _juegoRepository.DamePorOID(idJuego);
            juego.NombreJuego = nombreJuego;
            juego.Genero = genero;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.Modify()
            _juegoRepository.Modify(juego);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un juego por su ID.
        /// </summary>
        /// <param name="idJuego">ID del juego a eliminar</param>
        public void Eliminar(long idJuego)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.Destroy()
            _juegoRepository.Destroy(idJuego);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un juego por su identificador único.
        /// </summary>
        /// <param name="idJuego">ID del juego</param>
        /// <returns>Entidad Juego o null si no existe</returns>
        public Juego DamePorOID(long idJuego)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.DamePorOID()
            return _juegoRepository.DamePorOID(idJuego);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los juegos del catálogo.
        /// </summary>
        /// <returns>Lista de todos los juegos</returns>
        public IList<Juego> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → GenericRepository.DameTodos()
            return _juegoRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra juegos por nombre.
        /// </summary>
        /// <param name="filtro">Texto a buscar en el nombre del juego</param>
        /// <returns>Lista de juegos filtrados</returns>
        public IList<Juego> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/JuegoRepository.cs → DamePorFiltro()
            return _juegoRepository.DamePorFiltro(filtro);
        }
    }
}
