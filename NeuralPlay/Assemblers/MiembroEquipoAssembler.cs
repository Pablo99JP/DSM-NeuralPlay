using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class MiembroEquipoAssembler
    {
        public static MiembroEquipoViewModel? ConvertENToViewModel(MiembroEquipo? en)
        {
            if (en == null) return null;
            return new MiembroEquipoViewModel
            {
                IdMiembroEquipo = en.IdMiembroEquipo,
                IdUsuario = en.Usuario?.IdUsuario ?? 0,
                IdEquipo = en.Equipo?.IdEquipo ?? 0,
                Rol = en.Rol,
                Estado = en.Estado,
                FechaAlta = en.FechaAlta,
                FechaAccion = en.FechaAccion,
                FechaBaja = en.FechaBaja,
                NombreUsuario = en.Usuario?.Nick,
                NombreEquipo = en.Equipo?.Nombre,
                FotoPerfilUrl = en.Usuario?.Perfil?.FotoPerfilUrl
            };
        }

        public static IEnumerable<MiembroEquipoViewModel> ConvertListENToViewModel(IEnumerable<MiembroEquipo> list)
        {
            if (list == null) return Enumerable.Empty<MiembroEquipoViewModel>();
            return list.Select(ConvertENToViewModel)!
                       .Where(vm => vm != null)!;
        }
    }
}
