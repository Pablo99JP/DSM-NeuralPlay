using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace NeuralPlay.Models
{
    public class ProponerParticipacionViewModel
    {
        public Torneo Torneo { get; set; } = null!;
        public IEnumerable<Equipo> EquiposDisponibles { get; set; } = new List<Equipo>();
        public long SelectedEquipoId { get; set; }
        public string? Message { get; set; }
    }
}
