using ApplicationCore.Domain.EN;
using NeuralPlay.Models;
using System.Collections.Generic;
using System.Linq;

namespace NeuralPlay.Models.Assemblers
{
    public static class JuegoAssembler
    {
        public static JuegoViewModel ToViewModel(Juego juego)
        {
            if (juego == null)
            {
                // Devuelve null si la entidad es null para evitar excepciones.
                return null;
            }

            return new JuegoViewModel
            {
                IdJuego = juego.IdJuego,
                NombreJuego = juego.NombreJuego,
                Genero = juego.Genero
            };
        }

        public static IList<JuegoViewModel> ToViewModel(IEnumerable<Juego> juegos)
        {
            // Reutiliza el método de mapeo de una sola entidad.
            return juegos.Select(j => ToViewModel(j)).ToList();
        }
    }
}