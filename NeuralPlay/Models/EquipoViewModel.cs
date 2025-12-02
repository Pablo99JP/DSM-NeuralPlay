using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class EquipoViewModel
    {
        public long IdEquipo { get; set; }

        [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
        [Display(Name = "Nombre del Equipo")]
        public string? Nombre { get; set; }

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Fecha de creación")]
        public DateTime FechaCreacion { get; set; }

        [Display(Name = "Actividad")]
        [Required(ErrorMessage = "La actividad es obligatoria.")]
        public Actividad Actividad { get; set; }

        [Display(Name = "País")]
        public string? Pais { get; set; }

        [Display(Name = "Idioma")]
        public string? Idioma { get; set; }
    }
}
