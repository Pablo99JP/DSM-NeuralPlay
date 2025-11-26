using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class PublicacionAssembler
    {
        public static PublicacionViewModel ConvertENToViewModel(Publicacion en)
        {
            if (en == null) throw new System.ArgumentNullException(nameof(en));

            return new PublicacionViewModel
            {
                Id = (int)en.IdPublicacion,
                Contenido = en.Contenido ?? string.Empty,
                FechaCreacion = en.FechaCreacion,
                FechaEdicion = en.FechaEdicion,
                AutorId = en.Autor != null ? (int)en.Autor.IdUsuario : 0,
                ComunidadId = en.Comunidad != null ? (int)en.Comunidad.IdComunidad : 0
            };
        }

        public static IList<PublicacionViewModel> ConvertListENToViewModel(IEnumerable<Publicacion> ens)
        {
            if (ens == null) return new List<PublicacionViewModel>();
            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
