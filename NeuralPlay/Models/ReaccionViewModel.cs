using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class ReaccionViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Tipo")]
        public TipoReaccion Tipo { get; set; }

        [Display(Name = "Fecha creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaCreacion { get; set; }

        [ScaffoldColumn(false)]
        public int AutorId { get; set; }

        [ScaffoldColumn(false)]
        public int PublicacionId { get; set; }

        [ScaffoldColumn(false)]
        public int ComentarioId { get; set; }
    }
}
