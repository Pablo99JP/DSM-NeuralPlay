using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class MiembroComunidadAssembler
    {
        public static MiembroComunidadViewModel? ConvertENToViewModel(MiembroComunidad? en)
        {
            if (en == null) return null;
            return new MiembroComunidadViewModel
            {
                IdMiembroComunidad = en.IdMiembroComunidad,
                IdUsuario = en.Usuario?.IdUsuario ?? 0,
                IdComunidad = en.Comunidad?.IdComunidad ?? 0,
                Rol = en.Rol,
                Estado = en.Estado,
                FechaAlta = en.FechaAlta,
                FechaAccion = en.FechaAccion,
                FechaBaja = en.FechaBaja,
                NombreUsuario = en.Usuario?.Nick,
                NombreComunidad = en.Comunidad?.Nombre,
                Avatar = en.Usuario?.Perfil?.FotoPerfilUrl
            };
        }

        public static IEnumerable<MiembroComunidadViewModel> ConvertListENToViewModel(IEnumerable<MiembroComunidad> list)
        {
            if (list == null) return Enumerable.Empty<MiembroComunidadViewModel>();
            return list.Select(ConvertENToViewModel)!
                       .Where(vm => vm != null)!;
        }
    }
}
