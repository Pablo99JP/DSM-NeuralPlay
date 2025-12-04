using System.Collections.Generic;

namespace NeuralPlay.Models
{
    public class ChatEquipoViewModel
    {
        public long IdChatEquipo { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public IList<MensajeChatViewModel> Mensajes { get; set; } = new List<MensajeChatViewModel>();
    }
}