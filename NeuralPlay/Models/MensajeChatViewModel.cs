using System;

namespace NeuralPlay.Models
{
    public class MensajeChatViewModel
    {
        public string Contenido { get; set; } = string.Empty;
        public string NickAutor { get; set; } = string.Empty;
        public System.DateTime FechaEnvio { get; set; }
    }
}