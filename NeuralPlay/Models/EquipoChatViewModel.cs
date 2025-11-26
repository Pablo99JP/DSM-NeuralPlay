using System.Collections.Generic;

namespace NeuralPlay.Models
{
    public class EquipoChatViewModel
    {
        public long IdEquipo { get; set; }
        public string NombreEquipo { get; set; } = string.Empty;
        public IEnumerable<MensajeChatViewModel> Mensajes { get; set; } = new List<MensajeChatViewModel>();
    }
}