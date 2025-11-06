using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using NHibernate;

namespace Infrastructure.NHibernate.Repositories
{
    /// <summary>
    /// Repository concreto para Usuario: Implementa IUsuarioRepository con NHibernate.
    /// 
    /// Hereda de GenericRepository para obtener operaciones CRUD básicas:
    /// - DamePorOID(id): Buscar por ID
    /// - DameTodos(): Listar todos
    /// - New(entidad): Insertar
    /// - Modify(entidad): Actualizar
    /// - Destroy(id): Eliminar
    /// 
    /// Además implementa 3 READFILTERS CUSTOM:
    /// 1. DamePorFiltro(string): Búsqueda por Nick o Email (LIKE)
    /// 2. DamePorEquipo(long): Usuarios miembros de un equipo específico
    /// 3. DamePorComunidad(long): Usuarios miembros de una comunidad específica
    /// </summary>
    public class UsuarioRepository : GenericRepository<Usuario, long>, IUsuarioRepository
    {
        /// <summary>
        /// Constructor: Recibe ISession de NHibernate.
        /// La sesión es inyectada por DI y compartida por todos los repositories del mismo scope.
        /// </summary>
        /// <param name="session">Sesión de NHibernate para acceso a BD</param>
        public UsuarioRepository(ISession session) : base(session)
        {
        }

        /// <summary>
        /// READFILTER 1: Busca usuarios por Nick o Email usando LIKE (contiene).
        /// 
        /// LINQ to NHibernate traduce automáticamente:
        /// - .Query<Usuario>() → FROM Usuario
        /// - .Where(...) → WHERE ...
        /// - .Contains(filtro) → LIKE '%filtro%'
        /// - || (OR lógico) → OR en SQL
        /// - .ToList() → Ejecuta la query y materializa resultados
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT * FROM Usuario
        /// WHERE Nick LIKE '%filtro%' OR CorreoElectronico LIKE '%filtro%'
        /// 
        /// EJEMPLO DE USO:
        /// var usuarios = usuarioRepository.DamePorFiltro("john");
        /// // Retorna usuarios con Nick o Email que contengan "john"
        /// </summary>
        /// <param name="filtro">Texto a buscar en Nick o Email (case-insensitive en la mayoría de BD)</param>
        /// <returns>Lista de usuarios que coinciden con el filtro (puede estar vacía)</returns>
        public IList<Usuario> DamePorFiltro(string filtro)
        {
            // LINQ to NHibernate: traducción automática a SQL
            return _session.Query<Usuario>()
                .Where(u => u.Nick.Contains(filtro) || u.CorreoElectronico.Contains(filtro))
                .ToList();
        }

        /// <summary>
        /// READFILTER 2: Busca usuarios que son MIEMBROS de un equipo específico.
        /// 
        /// Usa navegación por RELACIONES de NHibernate:
        /// - Usuario tiene colección MiembrosEquipo (relación 1:N con MiembroEquipo)
        /// - MiembroEquipo tiene referencia Equipo (relación N:1 con Equipo)
        /// 
        /// LINQ to NHibernate traduce:
        /// - .Any(lambda) → EXISTS (subquery)
        /// - Navegación mem.Equipo.IdEquipo → JOIN automático
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT u.* FROM Usuario u
        /// WHERE EXISTS (
        ///     SELECT 1 FROM MiembroEquipo mem
        ///     WHERE mem.IdUsuario = u.IdUsuario
        ///     AND mem.IdEquipo = {idEquipo}
        /// )
        /// 
        /// EJEMPLO DE USO:
        /// var usuarios = usuarioRepository.DamePorEquipo(idEquipo: 1);
        /// // Retorna todos los usuarios que son miembros del equipo 1
        /// </summary>
        /// <param name="idEquipo">ID del equipo del cual obtener los miembros</param>
        /// <returns>Lista de usuarios miembros del equipo (puede estar vacía si el equipo no tiene miembros)</returns>
        public IList<Usuario> DamePorEquipo(long idEquipo)
        {
            // LINQ to NHibernate: .Any() genera EXISTS con subquery
            // Navegación por relaciones: u.MiembrosEquipo.Any(mem => mem.Equipo.IdEquipo == idEquipo)
            return _session.Query<Usuario>()
                .Where(u => u.MiembrosEquipo.Any(mem => mem.Equipo.IdEquipo == idEquipo))
                .ToList();
        }

        /// <summary>
        /// READFILTER 3: Busca usuarios que son MIEMBROS de una comunidad específica.
        /// 
        /// Similar a DamePorEquipo pero con relación MiembrosComunidad.
        /// 
        /// Usa navegación por RELACIONES de NHibernate:
        /// - Usuario tiene colección MiembrosComunidad (relación 1:N con MiembroComunidad)
        /// - MiembroComunidad tiene referencia Comunidad (relación N:1 con Comunidad)
        /// 
        /// LINQ to NHibernate traduce:
        /// - .Any(lambda) → EXISTS (subquery)
        /// - Navegación mem.Comunidad.IdComunidad → JOIN automático
        /// 
        /// SQL GENERADO (aproximado):
        /// SELECT u.* FROM Usuario u
        /// WHERE EXISTS (
        ///     SELECT 1 FROM MiembroComunidad mem
        ///     WHERE mem.IdUsuario = u.IdUsuario
        ///     AND mem.IdComunidad = {idComunidad}
        /// )
        /// 
        /// EJEMPLO DE USO:
        /// var usuarios = usuarioRepository.DamePorComunidad(idComunidad: 1);
        /// // Retorna todos los usuarios que son miembros de la comunidad 1
        /// </summary>
        /// <param name="idComunidad">ID de la comunidad de la cual obtener los miembros</param>
        /// <returns>Lista de usuarios miembros de la comunidad (puede estar vacía si la comunidad no tiene miembros)</returns>
        public IList<Usuario> DamePorComunidad(long idComunidad)
        {
            // LINQ to NHibernate: .Any() genera EXISTS con subquery
            // Navegación por relaciones: u.MiembrosComunidad.Any(mem => mem.Comunidad.IdComunidad == idComunidad)
            return _session.Query<Usuario>()
                .Where(u => u.MiembrosComunidad.Any(mem => mem.Comunidad.IdComunidad == idComunidad))
                .ToList();
        }
    }
}
