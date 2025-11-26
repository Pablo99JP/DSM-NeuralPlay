using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class ComentarioAssembler
    {
        public static ComentarioViewModel ConvertENToViewModel(Comentario en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new ComentarioViewModel
            {
                Id = (int)en.IdComentario,
                Contenido = en.Contenido ?? string.Empty,
                FechaCreacion = en.FechaCreacion,
                FechaEdicion = en.FechaEdicion,
                AutorId = en.Autor != null ? (int)en.Autor.IdUsuario : 0,
                PublicacionId = en.Publicacion != null ? (int)en.Publicacion.IdPublicacion : 0
            };
        }

        public static IList<ComentarioViewModel> ConvertListENToViewModel(IEnumerable<Comentario> ens)
        {
            if (ens == null) return new List<ComentarioViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
