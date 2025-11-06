using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// CP (Caso de Proceso / Use Case): Creación de una comunidad con su líder fundador.
    /// 
    /// Este CP garantiza que:
    /// 1. Se crea la Comunidad (usando ComunidadCEN)
    /// 2. Se crea la membresía del usuario fundador como LÍDER (usando MiembroComunidadCEN)
    /// 3. Ambas operaciones se confirman en UNA SOLA TRANSACCIÓN
    /// 
    /// REGLA DE NEGOCIO: Toda comunidad DEBE tener al menos un LÍDER desde su creación.
    /// Si falla cualquiera de los pasos → ROLLBACK completo.
    /// </summary>
    public class CrearComunidadCP
    {
        // Dependencias: CENs que se van a orquestar + UnitOfWork para transaccionalidad
        private readonly ComunidadCEN _comunidadCEN;
        private readonly MiembroComunidadCEN _miembroComunidadCEN;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// IMPORTANTE: Todos los CENs y UnitOfWork deben compartir la MISMA instancia de ISession.
        /// Configurar en DI como Scoped para garantizar que todos operen sobre la misma transacción.
        /// </summary>
        /// <param name="comunidadCEN">CEN para operaciones de Comunidad</param>
        /// <param name="miembroComunidadCEN">CEN para operaciones de MiembroComunidad</param>
        /// <param name="unitOfWork">UnitOfWork para gestión transaccional</param>
        public CrearComunidadCP(ComunidadCEN comunidadCEN, MiembroComunidadCEN miembroComunidadCEN, IUnitOfWork unitOfWork)
        {
            _comunidadCEN = comunidadCEN;
            _miembroComunidadCEN = miembroComunidadCEN;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Crea una nueva comunidad con un usuario fundador como LÍDER de forma TRANSACCIONAL.
        /// 
        /// FLUJO COMPLETO:
        /// 1. FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/ComunidadCEN.cs → Crear()
        ///    - Crea entidad Comunidad con nombre, descripción y fecha de creación
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 2. FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroComunidadCEN.cs (línea ~35) → Crear()
        ///    - Crea entidad MiembroComunidad con Rol=LIDER, Estado=ACTIVA
        ///    - REGLA DE NEGOCIO: FechaBaja=null, FechaAlta=DateTime.Now (aplicada automáticamente)
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 3. FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30) → SaveChanges()
        ///    - AQUÍ se ejecutan TODOS los SQL INSERT pendientes
        ///    - Se hace COMMIT de la transacción
        ///    - Si algo falla, se hace ROLLBACK automático (la comunidad NO se crea sin líder)
        /// 
        /// EJEMPLO DE USO:
        /// var idComunidad = crearComunidadCP.CrearComunidadConLider(
        ///     "Comunidad DSM", "Comunidad de desarrollo", idUsuarioFundador
        /// );
        /// // idComunidad contiene el ID de la comunidad creada
        /// // El usuario especificado ya es LÍDER de la comunidad
        /// </summary>
        /// <param name="nombre">Nombre de la comunidad (obligatorio)</param>
        /// <param name="descripcion">Descripción de la comunidad (obligatorio)</param>
        /// <param name="idUsuarioLider">ID del usuario que será el líder fundador (obligatorio)</param>
        /// <returns>ID de la comunidad creada</returns>
        /// <exception cref="Exception">
        /// Si cualquier operación falla (nombre duplicado, usuario no existe, error de BD, etc.),
        /// se propaga la excepción y se hace ROLLBACK automático.
        /// Garantiza que NO puede existir una comunidad sin líder.
        /// </exception>
        public long CrearComunidadConLider(string nombre, string descripcion, long idUsuarioLider)
        {
            // PASO 1: Crear Comunidad
            // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/ComunidadCEN.cs → Crear()
            var idComunidad = _comunidadCEN.Crear(
                nombre: nombre,
                fechaCreacion: DateTime.Now,  // Fecha actual del servidor
                descripcion: descripcion
            );
            // En este punto: Comunidad está marcada para INSERT pero NO está en BD todavía

            // PASO 2: Crear membresía del usuario fundador como LÍDER
            // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/MiembroComunidadCEN.cs (línea ~35)
            // REGLA DE NEGOCIO: El método Crear() automáticamente establece:
            // - FechaAlta = DateTime.Now
            // - FechaBaja = null (el líder NO tiene fecha de baja al crearse)
            _miembroComunidadCEN.Crear(
                rol: RolComunidad.LIDER,         // El fundador es LÍDER
                estado: EstadoMembresia.ACTIVA   // Estado activo desde el inicio
            );
            // En este punto: MiembroComunidad está marcado para INSERT pero NO está en BD todavía

            // PASO 3: COMMIT de la transacción - AQUÍ se ejecutan TODOS los SQL
            // FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
            // Ejecuta:
            // - INSERT INTO Comunidad (...)
            // - INSERT INTO MiembroComunidad (...)
            // - COMMIT (confirma ambos cambios)
            // Si cualquiera falla → ROLLBACK (garantiza que NO existe comunidad sin líder)
            _unitOfWork.SaveChanges();

            // Retorna el ID de la comunidad creada (disponible inmediatamente gracias a HiLo)
            return idComunidad;
        }
    }
}
