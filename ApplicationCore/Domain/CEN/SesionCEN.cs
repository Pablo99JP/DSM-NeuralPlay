using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Sesion.
    /// Expone operaciones CRUD para gestionar sesiones de usuario (autenticación).
    /// Representa una sesión activa con token para autenticación.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class SesionCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly ISesionRepository _sesionRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="sesionRepository">Implementación del repositorio de sesiones</param>
        public SesionCEN(ISesionRepository sesionRepository)
        {
            _sesionRepository = sesionRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea una nueva sesión de usuario.
        /// REGLA DE NEGOCIO: FechaInicio se establece automáticamente a DateTime.Now.
        /// REGLA DE NEGOCIO: FechaFin es null (sesión activa) hasta que se cierre.
        /// </summary>
        /// <param name="token">Token de autenticación (JWT, GUID, etc.)</param>
        /// <returns>ID de la sesión creada</returns>
        public long Crear(string token)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var sesion = new Sesion
            {
                FechaInicio = DateTime.Now,  // ← REGLA: Siempre fecha actual
                FechaFin = null,              // ← REGLA: null = sesión activa
                Token = token
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.New()
            _sesionRepository.New(sesion);
            
            return sesion.IdSesion;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica una sesión existente.
        /// Se usa principalmente para cerrar sesiones (establecer FechaFin).
        /// </summary>
        /// <param name="idSesion">ID de la sesión a modificar</param>
        /// <param name="fechaInicio">Fecha de inicio de la sesión</param>
        /// <param name="fechaFin">Fecha de fin (null si sigue activa)</param>
        /// <param name="token">Token de autenticación</param>
        public void Modificar(long idSesion, DateTime fechaInicio, DateTime? fechaFin, string token)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.DamePorOID()
            var sesion = _sesionRepository.DamePorOID(idSesion);
            
            // Actualiza las propiedades
            sesion.FechaInicio = fechaInicio;
            sesion.FechaFin = fechaFin;
            sesion.Token = token;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.Modify()
            _sesionRepository.Modify(sesion);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina una sesión por su ID.
        /// </summary>
        /// <param name="idSesion">ID de la sesión a eliminar</param>
        public void Eliminar(long idSesion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.Destroy()
            _sesionRepository.Destroy(idSesion);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene una sesión por su identificador único.
        /// </summary>
        /// <param name="idSesion">ID de la sesión</param>
        /// <returns>Entidad Sesion o null si no existe</returns>
        public Sesion DamePorOID(long idSesion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.DamePorOID()
            return _sesionRepository.DamePorOID(idSesion);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todas las sesiones del sistema.
        /// </summary>
        /// <returns>Lista de todas las sesiones</returns>
        public IList<Sesion> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.DameTodos()
            return _sesionRepository.DameTodos();
        }

        /// <summary>
        /// [CUSTOM METHOD] Cierra una sesión estableciendo FechaFin a DateTime.Now.
        /// Método de conveniencia para el caso de uso de logout.
        /// </summary>
        /// <param name="idSesion">ID de la sesión a cerrar</param>
        public void CerrarSesion(long idSesion)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.DamePorOID()
            var sesion = _sesionRepository.DamePorOID(idSesion);
            
            // REGLA: Establecer fecha de fin para marcar sesión como cerrada
            sesion.FechaFin = DateTime.Now;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/SesionRepository.cs → GenericRepository.Modify()
            _sesionRepository.Modify(sesion);
        }
    }
}
