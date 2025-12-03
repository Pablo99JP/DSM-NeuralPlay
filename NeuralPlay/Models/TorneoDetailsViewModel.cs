using System.Collections.Generic;
using ApplicationCore.Domain.EN;

namespace NeuralPlay.Models
{
    public class TorneoDetailsViewModel
    {
        public Torneo Torneo { get; set; } = null!;
        public IEnumerable<PropuestaTorneo> Propuestas { get; set; } = new List<PropuestaTorneo>();
        public IEnumerable<ParticipacionTorneo> Participaciones { get; set; } = new List<ParticipacionTorneo>();
        public IEnumerable<Equipo> EquiposUsuario { get; set; } = new List<Equipo>();
        public Dictionary<long, int> Clasificacion { get; set; } = new Dictionary<long, int>(); // IdEquipo -> Puntos
    }
}
