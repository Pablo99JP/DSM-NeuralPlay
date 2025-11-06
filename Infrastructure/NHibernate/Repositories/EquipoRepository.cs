using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    /// <summary>
    /// Repository concreto para Equipo: Implementa IEquipoRepository con NHibernate.
    /// 
    /// Hereda de GenericRepository para obtener operaciones CRUD básicas.
    /// 
    /// Además implementa 2 READFILTERS:
    /// 1. DamePorFiltro(string): Búsqueda por Nombre o Descripción (LIKE)
    /// 2. DamePorTorneo(long): Equipos que participan en un torneo específico
    /// </summary>
    public class EquipoRepository : GenericRepository<Equipo, long>, IEquipoRepository
    {
        /// <summary>
        /// Constructor: Recibe ISession de NHibernate compartida por todos los repositories del mismo scope.
        /// </summary>
        /// <param name="session">Sesión de NHibernate para acceso a BD</param>
        public EquipoRepository(ISession session) : base(session)
        {
        }

        /// <summary>
        /// READFILTER 1: Busca equipos por Nombre o Descripción usando LIKE (contiene).
        /// 
        /// LINQ to NHibernate traduce automáticamente:
        /// - .Contains(filtro) → LIKE '%filtro%'
        /// - || (OR lógico) → OR en SQL
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT * FROM Equipo
        /// WHERE Nombre LIKE '%filtro%' OR Descripcion LIKE '%filtro%'
        /// 
        /// EJEMPLO DE USO:
        /// var equipos = equipoRepository.DamePorFiltro("Dragons");
        /// // Retorna equipos con Nombre o Descripción que contengan "Dragons"
        /// </summary>
        /// <param name="filtro">Texto a buscar en Nombre o Descripción</param>
        /// <returns>Lista de equipos que coinciden con el filtro (puede estar vacía)</returns>
        public IList<Equipo> DamePorFiltro(string filtro)
        {
            // LINQ to NHibernate: traducción automática a SQL con LIKE
            return _session.Query<Equipo>()
                .Where(e => e.Nombre.Contains(filtro) || e.Descripcion.Contains(filtro))
                .ToList();
        }

        /// <summary>
        /// READFILTER 2: Busca equipos que PARTICIPAN en un torneo específico.
        /// 
        /// Usa navegación por RELACIONES de NHibernate:
        /// - Equipo tiene colección Participaciones (relación 1:N con ParticipacionTorneo)
        /// - ParticipacionTorneo tiene referencia Torneo (relación N:1 con Torneo)
        /// 
        /// LINQ to NHibernate traduce:
        /// - .Any(lambda) → EXISTS (subquery)
        /// - Navegación p.Torneo.IdTorneo → JOIN automático
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT e.* FROM Equipo e
        /// WHERE EXISTS (
        ///     SELECT 1 FROM ParticipacionTorneo p
        ///     WHERE p.IdEquipo = e.IdEquipo
        ///     AND p.IdTorneo = {idTorneo}
        /// )
        /// 
        /// EJEMPLO DE USO:
        /// var equipos = equipoRepository.DamePorTorneo(idTorneo: 1);
        /// // Retorna todos los equipos inscritos/participando en el torneo 1
        /// 
        /// NOTA: Esto incluye equipos en cualquier estado de participación:
        /// - EstadoParticipacion.ACEPTADA (confirmado)
        /// - EstadoParticipacion.PENDIENTE (en espera)
        /// - EstadoParticipacion.RECHAZADA (rechazado)
        /// Si se necesita filtrar por estado, agregar: .Where(...).And(p => p.Estado == EstadoParticipacion.ACEPTADA)
        /// </summary>
        /// <param name="idTorneo">ID del torneo del cual obtener los equipos participantes</param>
        /// <returns>Lista de equipos que participan en el torneo (puede estar vacía si el torneo no tiene participantes)</returns>
        public IList<Equipo> DamePorTorneo(long idTorneo)
        {
            // LINQ to NHibernate: .Any() genera EXISTS con subquery
            // Navegación por relaciones: e.Participaciones.Any(p => p.Torneo.IdTorneo == idTorneo)
            return _session.Query<Equipo>()
                .Where(e => e.Participaciones.Any(p => p.Torneo.IdTorneo == idTorneo))
                .ToList();
        }
    }
}
