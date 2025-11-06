using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad ChatEquipo.
    /// Expone operaciones CRUD para gestionar chats de equipos.
    /// Cada equipo tiene un chat único para comunicación interna.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class ChatEquipoCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IChatEquipoRepository _chatEquipoRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="chatEquipoRepository">Implementación del repositorio de chats de equipo</param>
        public ChatEquipoCEN(IChatEquipoRepository chatEquipoRepository)
        {
            _chatEquipoRepository = chatEquipoRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo chat para un equipo.
        /// REGLA DE NEGOCIO: Un equipo solo puede tener un chat.
        /// </summary>
        /// <returns>ID del chat creado</returns>
        public long Crear()
        {
            // Construye la entidad de dominio
            var chatEquipo = new ChatEquipo();

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.New()
            _chatEquipoRepository.New(chatEquipo);
            
            return chatEquipo.IdChatEquipo;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un chat de equipo existente.
        /// Nota: ChatEquipo tiene pocas propiedades modificables directamente.
        /// La mayoría de cambios se hacen a través de MensajeChatCEN.
        /// </summary>
        /// <param name="idChatEquipo">ID del chat a modificar</param>
        public void Modificar(long idChatEquipo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.DamePorOID()
            var chatEquipo = _chatEquipoRepository.DamePorOID(idChatEquipo);
            
            // Nota: ChatEquipo es una entidad simple, normalmente no se modifica directamente
            // Los cambios se hacen a través de sus relaciones (Equipo, Mensajes)

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.Modify()
            _chatEquipoRepository.Modify(chatEquipo);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un chat de equipo por su ID.
        /// ADVERTENCIA: Esto eliminará también todos los mensajes asociados si está configurado en cascade.
        /// </summary>
        /// <param name="idChatEquipo">ID del chat a eliminar</param>
        public void Eliminar(long idChatEquipo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.Destroy()
            _chatEquipoRepository.Destroy(idChatEquipo);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un chat de equipo por su identificador único.
        /// </summary>
        /// <param name="idChatEquipo">ID del chat</param>
        /// <returns>Entidad ChatEquipo o null si no existe</returns>
        public ChatEquipo DamePorOID(long idChatEquipo)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.DamePorOID()
            return _chatEquipoRepository.DamePorOID(idChatEquipo);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los chats de equipo del sistema.
        /// </summary>
        /// <returns>Lista de todos los chats de equipo</returns>
        public IList<ChatEquipo> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/ChatEquipoRepository.cs → GenericRepository.DameTodos()
            return _chatEquipoRepository.DameTodos();
        }
    }
}
