using System;
using System.ComponentModel.DataAnnotations;

namespace NeuralPlay.Models
{
    public class UsuarioViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Display(Name = "Fecha de registro")]
        [DataType(DataType.Date)]
        public DateTime FechaRegistro { get; set; }
    }
}
