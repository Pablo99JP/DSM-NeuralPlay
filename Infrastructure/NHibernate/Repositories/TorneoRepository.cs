using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    /// <summary>
    /// Repository concreto para Torneo: Implementa ITorneoRepository con NHibernate.
    /// 
    /// Hereda de GenericRepository para obtener operaciones CRUD básicas.
    /// 
    /// Además implementa 2 READFILTERS:
    /// 1. DamePorFiltro(string): Búsqueda por Nombre o Estado (LIKE)
    /// 2. DamePorEquipo(long): Torneos en los que participa un equipo específico
    /// </summary>
    public class TorneoRepository : GenericRepository<Torneo, long>, ITorneoRepository
    {
        /// <summary>
        /// Constructor: Recibe ISession de NHibernate compartida por todos los repositories del mismo scope.
        /// </summary>
        /// <param name="session">Sesión de NHibernate para acceso a BD</param>
        public TorneoRepository(ISession session) : base(session)
        {
        }

        /// <summary>
        /// READFILTER 1: Busca torneos por Nombre o Estado usando LIKE (contiene).
        /// 
        /// LINQ to NHibernate traduce automáticamente:
        /// - .Contains(filtro) → LIKE '%filtro%'
        /// - || (OR lógico) → OR en SQL
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT * FROM Torneo
        /// WHERE Nombre LIKE '%filtro%' OR Estado LIKE '%filtro%'
        /// 
        /// EJEMPLO DE USO:
        /// var torneos = torneoRepository.DamePorFiltro("Championship");
        /// // Retorna torneos con Nombre o Estado que contengan "Championship"
        /// 
        /// var torneos2 = torneoRepository.DamePorFiltro("ACTIVO");
        /// // Retorna torneos con Estado que contenga "ACTIVO"
        /// </summary>
        /// <param name="filtro">Texto a buscar en Nombre o Estado</param>
        /// <returns>Lista de torneos que coinciden con el filtro (puede estar vacía)</returns>
        public IList<Torneo> DamePorFiltro(string filtro)
        {
            // LINQ to NHibernate: traducción automática a SQL con LIKE
            return _session.Query<Torneo>()
                .Where(t => t.Nombre.Contains(filtro) || t.Estado.Contains(filtro))
                .ToList();
        }

        /// <summary>
        /// READFILTER 2: Busca torneos en los que PARTICIPA un equipo específico.
        /// 
        /// Usa navegación por RELACIONES de NHibernate:
        /// - Torneo tiene colección Participaciones (relación 1:N con ParticipacionTorneo)
        /// - ParticipacionTorneo tiene referencia Equipo (relación N:1 con Equipo)
        /// 
        /// LINQ to NHibernate traduce:
        /// - .Any(lambda) → EXISTS (subquery)
        /// - Navegación p.Equipo.IdEquipo → JOIN automático
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT t.* FROM Torneo t
        /// WHERE EXISTS (
        ///     SELECT 1 FROM ParticipacionTorneo p
        ///     WHERE p.IdTorneo = t.IdTorneo
        ///     AND p.IdEquipo = {idEquipo}
        /// )
        /// 
        /// EJEMPLO DE USO:
        /// var torneos = torneoRepository.DamePorEquipo(idEquipo: 1);
        /// // Retorna todos los torneos en los que participa el equipo 1
        /// 
        /// NOTA: Esto es la INVERSA de EquipoRepository.DamePorTorneo():
        /// - EquipoRepository.DamePorTorneo(idTorneo) → ¿Qué equipos están en este torneo?
        /// - TorneoRepository.DamePorEquipo(idEquipo) → ¿En qué torneos está este equipo?
        /// 
        /// Ambos usan la MISMA tabla intermedia (ParticipacionTorneo) pero desde perspectivas diferentes.
        /// </summary>
        /// <param name="idEquipo">ID del equipo del cual obtener sus torneos</param>
        /// <returns>Lista de torneos en los que participa el equipo (puede estar vacía si el equipo no participa en ningún torneo)</returns>
        public IList<Torneo> DamePorEquipo(long idEquipo)
        {
            // LINQ to NHibernate: .Any() genera EXISTS con subquery
            // Navegación por relaciones: t.Participaciones.Any(p => p.Equipo.IdEquipo == idEquipo)
            return _session.Query<Torneo>()
                .Where(t => t.Participaciones.Any(p => p.Equipo.IdEquipo == idEquipo))
                .ToList();
        }
    }
}
