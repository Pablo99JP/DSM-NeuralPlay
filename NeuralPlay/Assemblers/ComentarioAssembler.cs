using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
 public static class ComentarioAssembler
 {
 public static ComentarioViewModel? ConvertENToViewModel(Comentario? en)
 {
 if (en == null) return null;
 return new ComentarioViewModel
 {
 idComentario = en.IdComentario,
 publicacionId = en.Publicacion?.IdPublicacion ??0,
 contenido = en.Contenido,
 fechaCreacion = en.FechaCreacion,
 fechaEdicion = en.FechaEdicion ?? System.DateTime.MinValue,
 autorNick = en.Autor?.Nick,
 autorId = en.Autor != null ? (long)en.Autor.IdUsuario :0
 };
 }

 public static IList<ComentarioViewModel> ConvertListENToViewModel(IEnumerable<Comentario>? ens)
 {
 if (ens == null) return new List<ComentarioViewModel>();
 return ens.Select(ConvertENToViewModel).Where(c => c != null).Select(c => c!).ToList();
 }
 }
}
