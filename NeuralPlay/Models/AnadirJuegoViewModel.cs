using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class AnadirJuegoViewModel
    {
        [Required]
        public long IdPerfil { get; set; }

        [Required]
        [Display(Name = "Juego a añadir")]
        public long IdJuegoSeleccionado { get; set; }

        public IEnumerable<SelectListItem> ListaDeJuegos { get; set; } = new List<SelectListItem>();
    }
}