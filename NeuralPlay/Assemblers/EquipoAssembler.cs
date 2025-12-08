using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class EquipoAssembler
    {
        public static EquipoViewModel? ConvertENToViewModel(Equipo? en)
        {
            if (en == null) return null;
            return new EquipoViewModel
            {
                IdEquipo = en.IdEquipo,
                Nombre = en.Nombre,
                Descripcion = en.Descripcion,
                FechaCreacion = en.FechaCreacion,
                Actividad = en.Actividad,
                Pais = en.Pais,
                Idioma = en.Idioma,
                ImagenUrl = en.ImagenUrl,
                IsLeader = false
            };
        }

        public static IEnumerable<EquipoViewModel> ConvertListENToViewModel(IEnumerable<Equipo> list)
        {
            if (list == null) return Enumerable.Empty<EquipoViewModel>();
            return list.Select(ConvertENToViewModel)!
                       .Where(vm => vm != null)!;
        }
    }
}
