using System;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class TorneoCreateViewModel
    {
        [Required(ErrorMessage = "El nombre del torneo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        [Display(Name = "Nombre del Torneo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Inicio")]
        public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(7);

        [StringLength(1000, ErrorMessage = "Las reglas no pueden exceder los 1000 caracteres.")]
        [Display(Name = "Reglas")]
        public string? Reglas { get; set; }

        [StringLength(500, ErrorMessage = "Los premios no pueden exceder los 500 caracteres.")]
        [Display(Name = "Premios")]
        public string? Premios { get; set; }

        [Required(ErrorMessage = "Debes seleccionar una comunidad.")]
        [Display(Name = "Comunidad Organizadora")]
        public long ComunidadId { get; set; }
    }
}
