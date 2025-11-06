using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Usuario.
    /// Expone operaciones CRUD y métodos custom específicos de Usuario.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class UsuarioCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        // Permite testear este CEN con un mock, sin base de datos real
        private readonly IUsuarioRepository _usuarioRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// El framework DI (ASP.NET Core, etc.) resuelve automáticamente IUsuarioRepository
        /// y pasa la implementación concreta (UsuarioRepository con NHibernate).
        /// </summary>
        /// <param name="usuarioRepository">Implementación del repositorio de usuarios</param>
        public UsuarioCEN(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo usuario en el sistema.
        /// REGLA DE NEGOCIO: El usuario siempre se crea con EstadoCuenta.ACTIVA
        /// y FechaRegistro = DateTime.Now (no se permite especificar estos valores).
        /// </summary>
        /// <param name="nick">Nombre de usuario único (obligatorio)</param>
        /// <param name="correoElectronico">Email del usuario (obligatorio)</param>
        /// <param name="contrasenaHash">Contraseña hasheada (obligatorio - hashear ANTES de llamar)</param>
        /// <param name="telefono">Teléfono del usuario (opcional - puede ser null)</param>
        /// <returns>ID del usuario creado (generado por NHibernate con HiLo)</returns>
        public long Crear(string nick, string correoElectronico, string contrasenaHash, string telefono = null)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var usuario = new Usuario
            {
                Nick = nick,
                CorreoElectronico = correoElectronico,
                ContrasenaHash = contrasenaHash,
                FechaRegistro = DateTime.Now,              // ← REGLA: Siempre fecha actual
                EstadoCuenta = EstadoCuenta.ACTIVA,        // ← REGLA: Siempre ACTIVA al crear
                Telefono = telefono
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.New()
            // Marca la entidad para inserción en BD (no ejecuta SQL todavía, espera a UnitOfWork.SaveChanges())
            _usuarioRepository.New(usuario);
            
            // El ID ya está disponible después de New() porque NHibernate usa generador HiLo
            // (genera IDs localmente sin consultar BD, muy eficiente)
            return usuario.IdUsuario;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un usuario existente.
        /// Obtiene el usuario por ID, actualiza sus propiedades y persiste los cambios.
        /// </summary>
        /// <param name="idUsuario">ID del usuario a modificar</param>
        /// <param name="nick">Nuevo nombre de usuario</param>
        /// <param name="correoElectronico">Nuevo correo electrónico</param>
        /// <param name="contrasenaHash">Nueva contraseña hasheada</param>
        /// <param name="fechaRegistro">Fecha de registro (normalmente no se modifica)</param>
        /// <param name="estadoCuenta">Estado de la cuenta (ACTIVA, SUSPENDIDA, etc.)</param>
        /// <param name="telefono">Nuevo teléfono (opcional)</param>
        public void Modificar(long idUsuario, string nick, string correoElectronico, string contrasenaHash, 
            DateTime fechaRegistro, EstadoCuenta estadoCuenta, string telefono = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.DamePorOID()
            // Ejecuta: SELECT * FROM Usuario WHERE IdUsuario = @idUsuario
            var usuario = _usuarioRepository.DamePorOID(idUsuario);
            
            // Actualiza propiedades del objeto en memoria
            usuario.Nick = nick;
            usuario.CorreoElectronico = correoElectronico;
            usuario.ContrasenaHash = contrasenaHash;
            usuario.FechaRegistro = fechaRegistro;
            usuario.EstadoCuenta = estadoCuenta;
            usuario.Telefono = telefono;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.Modify()
            // Marca la entidad como modificada (SQL UPDATE se ejecuta en SaveChanges())
            _usuarioRepository.Modify(usuario);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un usuario del sistema.
        /// IMPORTANTE: Esto eliminará físicamente el registro de la BD.
        /// Para "soft delete" (desactivar sin borrar), usar Modificar() y cambiar EstadoCuenta.
        /// </summary>
        /// <param name="idUsuario">ID del usuario a eliminar</param>
        public void Eliminar(long idUsuario)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.Destroy()
            // Ejecuta: DELETE FROM Usuario WHERE IdUsuario = @idUsuario (en SaveChanges())
            _usuarioRepository.Destroy(idUsuario);
        }

        /// <summary>
        /// [CRUD - READ] Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="idUsuario">ID del usuario a buscar</param>
        /// <returns>Usuario encontrado o null si no existe</returns>
        public Usuario DamePorOID(long idUsuario)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.DamePorOID()
            // Ejecuta: SELECT * FROM Usuario WHERE IdUsuario = @idUsuario
            return _usuarioRepository.DamePorOID(idUsuario);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los usuarios del sistema.
        /// ADVERTENCIA: Si hay muchos usuarios, considera usar paginación.
        /// </summary>
        /// <returns>Lista con todos los usuarios</returns>
        public IList<Usuario> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.DameTodos()
            // Ejecuta: SELECT * FROM Usuario
            return _usuarioRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra usuarios por texto (busca en Nick o CorreoElectronico).
        /// Ejemplo: DamePorFiltro("player") → devuelve usuarios cuyo Nick o Email contenga "player"
        /// </summary>
        /// <param name="filtro">Texto a buscar en Nick o CorreoElectronico</param>
        /// <returns>Lista de usuarios que coinciden con el filtro</returns>
        public IList<Usuario> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → DamePorFiltro()
            // Ejecuta: SELECT * FROM Usuario WHERE Nick LIKE '%filtro%' OR CorreoElectronico LIKE '%filtro%'
            return _usuarioRepository.DamePorFiltro(filtro);
        }

        /// <summary>
        /// [CUSTOM] Método de autenticación de usuarios.
        /// REGLA DE NEGOCIO: Solo usuarios con EstadoCuenta.ACTIVA pueden hacer login.
        /// </summary>
        /// <param name="correoElectronico">Email del usuario</param>
        /// <param name="contrasenaHash">Contraseña hasheada (debe coincidir con la almacenada)</param>
        /// <returns>Usuario autenticado si las credenciales son correctas</returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Se lanza si:
        /// - El email no existe
        /// - La contraseña no coincide
        /// - La cuenta está inactiva (EstadoCuenta != ACTIVA)
        /// </exception>
        public Usuario Login(string correoElectronico, string contrasenaHash)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/UsuarioRepository.cs → GenericRepository.DameTodos()
            // Ejecuta: SELECT * FROM Usuario
            // OPTIMIZACIÓN POSIBLE: Crear un método DamePorEmail() para filtrar en BD, no en memoria
            var usuarios = _usuarioRepository.DameTodos();
            
            // LINQ to Objects: Filtra en memoria
            // FirstOrDefault() devuelve el primer usuario que cumple las condiciones o null
            var usuario = usuarios.FirstOrDefault(u => 
                u.CorreoElectronico == correoElectronico &&      // Validación 1: Email coincide
                u.ContrasenaHash == contrasenaHash &&            // Validación 2: Contraseña coincide
                u.EstadoCuenta == EstadoCuenta.ACTIVA);          // Validación 3: Cuenta ACTIVA

            // Validación final: Si no se encontró usuario, credenciales inválidas
            if (usuario == null)
            {
                // Lanza excepción que será capturada en el Controller
                // El Controller devolverá HTTP 401 Unauthorized
                throw new UnauthorizedAccessException("Credenciales inválidas o cuenta inactiva.");
            }

            // Login exitoso: Retorna el usuario autenticado
            return usuario;
        }
    }
}
