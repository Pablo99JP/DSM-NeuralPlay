using System.Collections.Generic;

namespace NeuralPlay.Models
{
    public class ExplorarViewModel
    {
        public IEnumerable<ComunidadViewModel> Comunidades { get; set; } = new List<ComunidadViewModel>();
        public IEnumerable<EquipoViewModel> Equipos { get; set; } = new List<EquipoViewModel>();
    }
}
