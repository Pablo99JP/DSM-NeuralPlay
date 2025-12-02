using ApplicationCore.Domain.EN;
using NeuralPlay.Models;
using System.Linq;

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
                fechaEdicion = en.FechaEdicion ?? DateTime.MinValue,
                IdComunidad = en.Comunidad?.IdComunidad,
                NombreComunidad = en.Comunidad?.Nombre,
                IdAutor = en.Autor?.IdUsuario,
                NickAutor = en.Autor?.Nick,
                LikeCount = en.Reacciones?.Count(r => r.Tipo == ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA) ??0,
                LikedByUser = false // controller will set this based on session
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
