using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class JuegoViewModel
    {
        public long IdJuego { get; set; }

        [Required(ErrorMessage = "El nombre del juego es obligatorio.")]
        [Display(Name = "Nombre del Juego")]
        public string? NombreJuego { get; set; }

        [Required(ErrorMessage = "El género del juego es obligatorio.")]
        [Display(Name = "Género")]
        public GeneroJuego Genero { get; set; }
    }
}