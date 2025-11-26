using System;
using System.ComponentModel.DataAnnotations;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
    public class SolicitudIngresoViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Display(Name = "Tipo")]
        public TipoInvitacion Tipo { get; set; }

        [Display(Name = "Estado")]
        public EstadoSolicitud Estado { get; set; }

        [Display(Name = "Fecha solicitud")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaSolicitud { get; set; }

        [Display(Name = "Fecha resolución")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? FechaResolucion { get; set; }

        [Display(Name = "Solicitante (Id)")]
        public int SolicitanteId { get; set; }

        [Display(Name = "Comunidad (Id)")]
        public int ComunidadId { get; set; }

        [Display(Name = "Equipo (Id)")]
        public int EquipoId { get; set; }
    }
}
