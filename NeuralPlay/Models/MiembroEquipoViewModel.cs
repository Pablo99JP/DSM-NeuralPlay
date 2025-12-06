using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class MiembroEquipoViewModel
    {
        public long IdMiembroEquipo { get; set; }

        [Display(Name = "Usuario")]
        public long IdUsuario { get; set; }

        [Display(Name = "Equipo")]
        public long IdEquipo { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public RolEquipo Rol { get; set; }

        [Display(Name = "Estado")]
        public EstadoMembresia Estado { get; set; }

        [Display(Name = "Fecha de Alta")]
        public DateTime FechaAlta { get; set; }

        [Display(Name = "Fecha de Acción")]
        public DateTime? FechaAccion { get; set; }

        [Display(Name = "Fecha de Baja")]
        public DateTime? FechaBaja { get; set; }

        // Propiedades para visualización
        [Display(Name = "Nombre del Usuario")]
        public string? NombreUsuario { get; set; }

        [Display(Name = "Nombre del Equipo")]
        public string? NombreEquipo { get; set; }

        public string? FotoPerfilUrl { get; set; }
    }
}
