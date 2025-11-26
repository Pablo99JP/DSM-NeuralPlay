using System;
using ApplicationCore.Domain.Enums;

namespace NeuralPlay.Models
{
 public class ReaccionViewModel
 {
 public int Id { get; set; }
 public TipoReaccion Tipo { get; set; }
 public DateTime Fecha { get; set; }
 public long AutorId { get; set; }
 public long PublicacionId { get; set; }
 public long ComentarioId { get; set; }
 }
}
