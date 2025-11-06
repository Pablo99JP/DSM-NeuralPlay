using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad Perfil.
    /// Expone operaciones CRUD y filtros para gestionar perfiles de usuarios.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class PerfilCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IPerfilRepository _perfilRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="perfilRepository">Implementación del repositorio de perfiles</param>
        public PerfilCEN(IPerfilRepository perfilRepository)
        {
            _perfilRepository = perfilRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo perfil de usuario.
        /// </summary>
        /// <param name="visibilidadPerfil">Visibilidad del perfil (PUBLICA, PRIVADA, AMIGOS)</param>
        /// <param name="visibilidadActividad">Visibilidad de la actividad (PUBLICA, PRIVADA, AMIGOS)</param>
        /// <param name="fotoPerfilUrl">URL de la foto de perfil (opcional)</param>
        /// <param name="descripcion">Descripción del perfil (opcional)</param>
        /// <param name="juegoFavoritoId">ID del juego favorito (opcional)</param>
        /// <returns>ID del perfil creado</returns>
        public long Crear(Visibilidad visibilidadPerfil, Visibilidad visibilidadActividad, string fotoPerfilUrl = null, string descripcion = null, long? juegoFavoritoId = null)
        {
            var perfil = new Perfil
            {
                VisibilidadPerfil = visibilidadPerfil,
                VisibilidadActividad = visibilidadActividad,
                FotoPerfilUrl = fotoPerfilUrl,
                Descripcion = descripcion,
                JuegoFavoritoId = juegoFavoritoId
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.New()
            _perfilRepository.New(perfil);
            return perfil.IdPerfil;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un perfil existente.
        /// </summary>
        /// <param name="idPerfil">ID del perfil a modificar</param>
        /// <param name="visibilidadPerfil">Nueva visibilidad del perfil</param>
        /// <param name="visibilidadActividad">Nueva visibilidad de actividad</param>
        /// <param name="fotoPerfilUrl">Nueva URL de foto (opcional)</param>
        /// <param name="descripcion">Nueva descripción (opcional)</param>
        /// <param name="juegoFavoritoId">Nuevo juego favorito (opcional)</param>
        public void Modificar(long idPerfil, Visibilidad visibilidadPerfil, Visibilidad visibilidadActividad, string fotoPerfilUrl = null, string descripcion = null, long? juegoFavoritoId = null)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.DamePorOID()
            var perfil = _perfilRepository.DamePorOID(idPerfil);
            perfil.VisibilidadPerfil = visibilidadPerfil;
            perfil.VisibilidadActividad = visibilidadActividad;
            perfil.FotoPerfilUrl = fotoPerfilUrl;
            perfil.Descripcion = descripcion;
            perfil.JuegoFavoritoId = juegoFavoritoId;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.Modify()
            _perfilRepository.Modify(perfil);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un perfil por su ID.
        /// </summary>
        /// <param name="idPerfil">ID del perfil a eliminar</param>
        public void Eliminar(long idPerfil)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.Destroy()
            _perfilRepository.Destroy(idPerfil);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un perfil por su identificador único.
        /// </summary>
        /// <param name="idPerfil">ID del perfil</param>
        /// <returns>Entidad Perfil o null si no existe</returns>
        public Perfil DamePorOID(long idPerfil)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.DamePorOID()
            return _perfilRepository.DamePorOID(idPerfil);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los perfiles del sistema.
        /// </summary>
        /// <returns>Lista de todos los perfiles</returns>
        public IList<Perfil> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → GenericRepository.DameTodos()
            return _perfilRepository.DameTodos();
        }

        /// <summary>
        /// [READ FILTER] Filtra perfiles por descripción.
        /// </summary>
        /// <param name="filtro">Texto a buscar en la descripción</param>
        /// <returns>Lista de perfiles filtrados</returns>
        public IList<Perfil> DamePorFiltro(string filtro)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/PerfilRepository.cs → DamePorFiltro()
            return _perfilRepository.DamePorFiltro(filtro);
        }
    }
}
