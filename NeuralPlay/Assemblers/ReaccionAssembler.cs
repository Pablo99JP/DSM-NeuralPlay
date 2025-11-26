using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
 public static class ReaccionAssembler
 {
 public static ReaccionViewModel ConvertENToViewModel(Reaccion en)
 {
 if (en == null) return null!;
 return new ReaccionViewModel
 {
 Id = (int)en.IdReaccion,
 Tipo = en.Tipo,
 Fecha = en.FechaCreacion,
 AutorId = en.Autor?.IdUsuario ??0,
 PublicacionId = en.Publicacion?.IdPublicacion ??0,
 ComentarioId = en.Comentario?.IdComentario ??0
 };
 }
 }
}
