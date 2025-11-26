using System;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
 public class SolicitudIngresoViewModel
 {
 public long IdSolicitud { get; set; }
 public TipoInvitacion Tipo { get; set; }
 public EstadoSolicitud Estado { get; set; }
 public DateTime FechaSolicitud { get; set; }
 public DateTime? FechaResolucion { get; set; }

 public long? SolicitanteId { get; set; }
 public string? SolicitanteNick { get; set; }

 public long? ComunidadId { get; set; }
 public string? ComunidadNombre { get; set; }
 public long? EquipoId { get; set; }
 public string? EquipoNombre { get; set; }
 }
}
