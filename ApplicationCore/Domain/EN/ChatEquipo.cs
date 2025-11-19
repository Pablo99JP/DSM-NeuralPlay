using System.Collections.Generic;

namespace ApplicationCore.Domain.EN
{
    public class ChatEquipo
    {
        public virtual long IdChatEquipo { get; set; }
        public virtual IList<MensajeChat> Mensajes { get; set; } = new List<MensajeChat>();
    }
}
