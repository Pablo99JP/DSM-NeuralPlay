using System;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    /// <summary>
    /// CP (Caso de Proceso / Use Case): Registro completo de un usuario.
    /// Los CPs orquestan MÚLTIPLES CENs y garantizan TRANSACCIONALIDAD.
    /// 
    /// Este CP realiza:
    /// 1. Crear Usuario (usando UsuarioCEN)
    /// 2. Crear Perfil asociado (usando PerfilCEN)
    /// 3. Confirmar ambas operaciones en UNA SOLA TRANSACCIÓN
    /// 
    /// Si cualquiera de los pasos falla, TODA la operación se revierte (ROLLBACK).
    /// </summary>
    public class RegistroUsuarioCP
    {
        // Dependencias: CENs que se van a orquestar + UnitOfWork para transaccionalidad
        private readonly UsuarioCEN _usuarioCEN;
        private readonly PerfilCEN _perfilCEN;
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// El framework DI resuelve automáticamente los CENs y el UnitOfWork.
        /// IMPORTANTE: Todos deben compartir la MISMA instancia de ISession (configurar como Scoped en DI).
        /// </summary>
        /// <param name="usuarioCEN">CEN para operaciones de Usuario</param>
        /// <param name="perfilCEN">CEN para operaciones de Perfil</param>
        /// <param name="unitOfWork">UnitOfWork para gestión transaccional</param>
        public RegistroUsuarioCP(UsuarioCEN usuarioCEN, PerfilCEN perfilCEN, IUnitOfWork unitOfWork)
        {
            _usuarioCEN = usuarioCEN;
            _perfilCEN = perfilCEN;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Registra un nuevo usuario con su perfil asociado de forma TRANSACCIONAL.
        /// 
        /// FLUJO COMPLETO:
        /// 1. FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~35) → Crear()
        ///    - Crea entidad Usuario con reglas de negocio (EstadoCuenta=ACTIVA, FechaRegistro=NOW)
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 2. FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/PerfilCEN.cs → Crear()
        ///    - Crea entidad Perfil con visibilidad por defecto
        ///    - Marca para INSERT en BD (no ejecuta SQL todavía)
        /// 
        /// 3. FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30) → SaveChanges()
        ///    - AQUÍ se ejecutan TODOS los SQL INSERT pendientes
        ///    - Se hace COMMIT de la transacción
        ///    - Si algo falla, se hace ROLLBACK automático (ningún cambio se guarda)
        /// 
        /// EJEMPLO DE USO:
        /// var idUsuario = registroUsuarioCP.RegistrarUsuarioConPerfil(
        ///     "newuser", "newuser@test.com", "hashed_password", "123456789"
        /// );
        /// // idUsuario contiene el ID del usuario creado
        /// // El perfil también fue creado automáticamente
        /// </summary>
        /// <param name="nick">Nombre de usuario (obligatorio)</param>
        /// <param name="correoElectronico">Email del usuario (obligatorio)</param>
        /// <param name="contrasenaHash">Contraseña hasheada (obligatorio)</param>
        /// <param name="telefono">Teléfono (opcional)</param>
        /// <returns>ID del usuario creado</returns>
        /// <exception cref="Exception">
        /// Si cualquier operación falla (email duplicado, error de BD, etc.),
        /// se propaga la excepción y se hace ROLLBACK automático.
        /// </exception>
        public long RegistrarUsuarioConPerfil(string nick, string correoElectronico, string contrasenaHash, string telefono = null)
        {
            // PASO 1: Crear Usuario
            // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/UsuarioCEN.cs (línea ~35)
            var idUsuario = _usuarioCEN.Crear(
                nick: nick,
                correoElectronico: correoElectronico,
                contrasenaHash: contrasenaHash,
                telefono: telefono
            );
            // En este punto: Usuario está marcado para INSERT pero NO está en BD todavía

            // PASO 2: Crear Perfil asociado al usuario
            // FLUJO SE DESPLAZA A: ApplicationCore/Domain/CEN/PerfilCEN.cs → Crear()
            _perfilCEN.Crear(
                visibilidadPerfil: Visibilidad.PUBLICO,      // Valor por defecto
                visibilidadActividad: Visibilidad.PUBLICO    // Valor por defecto
            );
            // En este punto: Perfil está marcado para INSERT pero NO está en BD todavía

            // PASO 3: COMMIT de la transacción - AQUÍ se ejecutan TODOS los SQL
            // FLUJO SE DESPLAZA A: Infrastructure/UnitOfWork/NHibernateUnitOfWork.cs (línea ~30)
            // Ejecuta:
            // - INSERT INTO Usuario (...)
            // - INSERT INTO Perfil (...)
            // - COMMIT (confirma ambos cambios)
            // Si cualquiera falla → ROLLBACK (se deshacen ambos cambios)
            _unitOfWork.SaveChanges();

            // Retorna el ID del usuario creado (disponible inmediatamente gracias a HiLo)
            return idUsuario;
        }
    }
}
