using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class ComunidadAssembler
    {
        public static ComunidadViewModel? ConvertENToViewModel(Comunidad? en)
        {
            if (en == null) return null;
            // Depuración: mostrar el valor recibido de ImagenUrl en consola
            System.Diagnostics.Debug.WriteLine($"[ComunidadAssembler] Comunidad: {en?.Nombre}, ImagenUrl: {en?.ImagenUrl}");
            return new ComunidadViewModel
            {
                IdComunidad = en.IdComunidad,
                Nombre = en.Nombre,
                Descripcion = en.Descripcion,
                FechaCreacion = en.FechaCreacion,
                ImagenUrl = en.ImagenUrl
            };
        }

        public static IEnumerable<ComunidadViewModel> ConvertListENToViewModel(IEnumerable<Comunidad> list)
        {
            if (list == null) return Enumerable.Empty<ComunidadViewModel>();
            return list.Select(ConvertENToViewModel)!
                       .Where(vm => vm != null)!;
        }
    }
}
