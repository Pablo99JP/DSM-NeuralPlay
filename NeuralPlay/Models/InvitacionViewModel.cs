using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class InvitacionViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Tipo")]
        public TipoInvitacion Tipo { get; set; }

        [Display(Name = "Estado")]
        public EstadoSolicitud Estado { get; set; }

        [Display(Name = "Fecha envío")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaEnvio { get; set; }

        [Display(Name = "Fecha respuesta")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaRespuesta { get; set; }

        [Display(Name = "Emisor")]
        public int EmisorId { get; set; }

        [Required(ErrorMessage = "El destinatario es obligatorio.")]
        [Display(Name = "Destinatario (Id usuario)")]
        public int DestinatarioId { get; set; }

        [Display(Name = "Comunidad (Id)")]
        public int ComunidadId { get; set; }

        [Display(Name = "Equipo (Id)")]
        public int EquipoId { get; set; }
    }
}
