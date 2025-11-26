using System;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
 public class InvitacionViewModel
 {
 public long IdInvitacion { get; set; }
 public TipoInvitacion Tipo { get; set; }
 public EstadoSolicitud Estado { get; set; }
 public DateTime FechaEnvio { get; set; }
 public DateTime? FechaRespuesta { get; set; }

 public long? EmisorId { get; set; }
 public string? EmisorNick { get; set; }
 public long? DestinatarioId { get; set; }
 public string? DestinatarioNick { get; set; }

 public long? ComunidadId { get; set; }
 public string? ComunidadNombre { get; set; }
 public long? EquipoId { get; set; }
 public string? EquipoNombre { get; set; }
 }
}
