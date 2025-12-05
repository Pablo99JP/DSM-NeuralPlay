using System.Collections.Generic;

namespace NeuralPlay.Models
{
    public class ExplorarViewModel
    {
        public IEnumerable<ComunidadViewModel> Comunidades { get; set; } = new List<ComunidadViewModel>();
        public IEnumerable<EquipoViewModel> Equipos { get; set; } = new List<EquipoViewModel>();
        public IEnumerable<JuegoViewModel> Juegos { get; set; } = new List<JuegoViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
    }
}
