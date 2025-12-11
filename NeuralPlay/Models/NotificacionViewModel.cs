using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class NotificacionViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Texto")]
        public string Texto { get; set; } = string.Empty;

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Leída")]
        public bool Leido { get; set; }

        [ScaffoldColumn(false)]
        public TipoNotificacion Tipo { get; set; }

        [ScaffoldColumn(false)]
        public int UsuarioId { get; set; }
    }
}
