using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    /// <summary>
    /// CEN (Componente Entidad Negocio) para la entidad MensajeChat.
    /// Expone operaciones CRUD para gestionar mensajes en chats de equipos.
    /// NO contiene lógica transaccional compleja (eso va en los CPs).
    /// </summary>
    public class MensajeChatCEN
    {
        // Dependencia: Interfaz del repositorio (NO implementación concreta)
        private readonly IMensajeChatRepository _mensajeChatRepository;

        /// <summary>
        /// Constructor: Inyección de dependencias.
        /// </summary>
        /// <param name="mensajeChatRepository">Implementación del repositorio de mensajes de chat</param>
        public MensajeChatCEN(IMensajeChatRepository mensajeChatRepository)
        {
            _mensajeChatRepository = mensajeChatRepository;
        }

        /// <summary>
        /// [CRUD - CREATE] Crea un nuevo mensaje en un chat.
        /// REGLA DE NEGOCIO: FechaEnvio se establece automáticamente a DateTime.Now.
        /// </summary>
        /// <param name="contenido">Contenido del mensaje (obligatorio)</param>
        /// <returns>ID del mensaje creado</returns>
        public long Crear(string contenido)
        {
            // Construye la entidad de dominio aplicando reglas de negocio
            var mensajeChat = new MensajeChat
            {
                Contenido = contenido,
                FechaEnvio = DateTime.Now  // ← REGLA: Siempre fecha actual
            };

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.New()
            _mensajeChatRepository.New(mensajeChat);
            
            return mensajeChat.IdMensajeChat;
        }

        /// <summary>
        /// [CRUD - UPDATE] Modifica un mensaje de chat existente.
        /// Normalmente los mensajes no se modifican, pero se permite para casos de edición.
        /// </summary>
        /// <param name="idMensajeChat">ID del mensaje a modificar</param>
        /// <param name="contenido">Nuevo contenido del mensaje</param>
        /// <param name="fechaEnvio">Fecha de envío original</param>
        public void Modificar(long idMensajeChat, string contenido, DateTime fechaEnvio)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.DamePorOID()
            var mensajeChat = _mensajeChatRepository.DamePorOID(idMensajeChat);
            
            // Actualiza las propiedades
            mensajeChat.Contenido = contenido;
            mensajeChat.FechaEnvio = fechaEnvio;

            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.Modify()
            _mensajeChatRepository.Modify(mensajeChat);
        }

        /// <summary>
        /// [CRUD - DELETE] Elimina un mensaje de chat por su ID.
        /// </summary>
        /// <param name="idMensajeChat">ID del mensaje a eliminar</param>
        public void Eliminar(long idMensajeChat)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.Destroy()
            _mensajeChatRepository.Destroy(idMensajeChat);
        }

        /// <summary>
        /// [CRUD - READ BY ID] Obtiene un mensaje de chat por su identificador único.
        /// </summary>
        /// <param name="idMensajeChat">ID del mensaje</param>
        /// <returns>Entidad MensajeChat o null si no existe</returns>
        public MensajeChat DamePorOID(long idMensajeChat)
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.DamePorOID()
            return _mensajeChatRepository.DamePorOID(idMensajeChat);
        }

        /// <summary>
        /// [CRUD - READ ALL] Obtiene todos los mensajes de chat del sistema.
        /// </summary>
        /// <returns>Lista de todos los mensajes de chat</returns>
        public IList<MensajeChat> DameTodos()
        {
            // FLUJO SE DESPLAZA A: Infrastructure/NHibernate/Repositories/MensajeChatRepository.cs → GenericRepository.DameTodos()
            return _mensajeChatRepository.DameTodos();
        }
    }
}
