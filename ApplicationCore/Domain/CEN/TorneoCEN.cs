using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Torneo.
    /// Expone operaciones CRUD y filtros para gestionar torneos competitivos.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class TorneoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly ITorneoRepository _torneoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="torneoRepository">Implementación del repositorio de torneos</param>
        public TorneoCEN(ITorneoRepository torneoRepository)
        {
            _torneoRepository = torneoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo torneo.
        /// </summary>
        /// <param name="nombre">Nombre del torneo (obligatorio)</param>
        /// <param name="fechaInicio">Fecha de inicio del torneo</param>
        /// <param name="estado">Estado del torneo (PENDIENTE, EN_CURSO, FINALIZADO)</param>
        /// <param name="reglas">Reglas del torneo (opcional)</param>
        /// <returns>ID del torneo creado</returns>
        public long Crear(string nombre, DateTime fechaInicio, string estado, string reglas = null)
        {
            var torneo = new Torneo
            {
                Nombre = nombre,
                FechaInicio = fechaInicio,
                Estado = estado,
                Reglas = reglas
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.New()
            _torneoRepository.New(torneo);
            return torneo.IdTorneo;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un torneo existente.
        /// </summary>
        /// <param name="idTorneo">ID del torneo a modificar</param>
        /// <param name="nombre">Nuevo nombre</param>
        /// <param name="fechaInicio">Nueva fecha de inicio</param>
        /// <param name="estado">Nuevo estado</param>
        /// <param name="reglas">Nuevas reglas (opcional)</param>
        public void Modificar(long idTorneo, string nombre, DateTime fechaInicio, string estado, string reglas = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.DamePorOID()
            var torneo = _torneoRepository.DamePorOID(idTorneo);
            torneo.Nombre = nombre;
            torneo.FechaInicio = fechaInicio;
            torneo.Estado = estado;
            torneo.Reglas = reglas;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.Modify()
            _torneoRepository.Modify(torneo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un torneo por su ID.
        /// </summary>
        /// <param name="idTorneo">ID del torneo a eliminar</param>
        public void Eliminar(long idTorneo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.Destroy()
            _torneoRepository.Destroy(idTorneo);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un torneo por su identificador único.
        /// </summary>
        /// <param name="idTorneo">ID del torneo</param>
        /// <returns>Entidad Torneo o null si no existe</returns>
        public Torneo DamePorOID(long idTorneo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.DamePorOID()
            return _torneoRepository.DamePorOID(idTorneo);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los torneos del sistema.
        /// </summary>
        /// <returns>Lista de todos los torneos</returns>
        public IList<Torneo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → GenericRepository.DameTodos()
            return _torneoRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra torneos por nombre o estado.
        /// </summary>
        /// <param name="filtro">Texto a buscar en nombre o estado</param>
        /// <returns>Lista de torneos filtrados</returns>
        public IList<Torneo> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/TorneoRepository.cs → DamePorFiltro()
            return _torneoRepository.DamePorFiltro(filtro);
        }
    }
}
