using ApplicationCore.Domain.EN;
using System.Collections.Generic;

namespace NeuralPlay.Models
{
    public class PropuestaTorneoDetailsViewModel
    {
        public PropuestaTorneo Propuesta { get; set; } = null!;
        public IEnumerable<Usuario> MiembrosEquipo { get; set; } = new List<Usuario>();
    }
}
