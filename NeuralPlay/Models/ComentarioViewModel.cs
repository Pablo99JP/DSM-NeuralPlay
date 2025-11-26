using System;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class ComentarioViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El contenido es obligatorio.")]
        [Display(Name = "Contenido")]
        public string Contenido { get; set; } = string.Empty;

        [Display(Name = "Fecha creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Fecha edición")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaEdicion { get; set; }

        [ScaffoldColumn(false)]
        public int AutorId { get; set; }

        [ScaffoldColumn(false)]
        public int PublicacionId { get; set; }
    }
}
