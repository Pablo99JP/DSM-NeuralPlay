using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class ChatEquipo
    {
        public virtual long IdChatEquipo { get; set; }
        
        public virtual Equipo Equipo { get; set; }
        public virtual ICollection<MensajeChat> Mensajes { get; set; }
        
        public ChatEquipo()
        {
            Mensajes = new List<MensajeChat>();
        }
    }
}
