using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public class PublicacionAssembler
    {
        public static PublicacionViewModel? ConvertENToViewModel(Publicacion en)
        {
            if (en == null) return null;
            return new PublicacionViewModel
            {
                idPublicacion = en.IdPublicacion,
                contenido = en.Contenido,
                fechaCreacion = en.FechaCreacion,
                fechaEdicion = en.FechaEdicion ?? DateTime.MinValue
            };
        }

        public static IEnumerable<PublicacionViewModel> ConvertListENToViewModel(IEnumerable<Publicacion> list)
        {
            if (list == null) return Enumerable.Empty<PublicacionViewModel>();
            return list.Select(ConvertENToViewModel)!
                       .Where(vm => vm != null)!;
        }
    }
}
