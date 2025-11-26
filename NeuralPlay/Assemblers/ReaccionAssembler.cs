using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class ReaccionAssembler
    {
        public static ReaccionViewModel ConvertENToViewModel(Reaccion en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new ReaccionViewModel
            {
                Id = (int)en.IdReaccion,
                Tipo = en.Tipo,
                FechaCreacion = en.FechaCreacion,
                AutorId = en.Autor != null ? (int)en.Autor.IdUsuario : 0,
                PublicacionId = en.Publicacion != null ? (int)en.Publicacion.IdPublicacion : 0,
                ComentarioId = en.Comentario != null ? (int)en.Comentario.IdComentario : 0
            };
        }

        public static IList<ReaccionViewModel> ConvertListENToViewModel(IEnumerable<Reaccion> ens)
        {
            if (ens == null) return new List<ReaccionViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
