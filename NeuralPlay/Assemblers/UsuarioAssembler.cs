using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using NeuralPlay.Models;

namespace NeuralPlay.Assemblers
{
    public static class UsuarioAssembler
    {
        /// <summary>
        /// Convierte una entidad UsuarioEN a UsuarioViewModel mapeando propiedad por propiedad.
        /// </summary>
        public static UsuarioViewModel ConvertENToViewModel(UsuarioEN en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en)); // Fail fast si se pasa null

            return new UsuarioViewModel
            {
                Id = en.Id,
                Email = en.Email ?? string.Empty,
                Password = en.Password ?? string.Empty,
                Nombre = en.Nombre ?? string.Empty,
                Apellidos = en.Apellidos ?? string.Empty,
                FechaRegistro = en.FechaRegistro
            };
        }

        /// <summary>
        /// Convierte una lista de UsuarioEN a una lista de UsuarioViewModel (mapeo manual).
        /// </summary>
        public static IList<UsuarioViewModel> ConvertListENToModel(IList<UsuarioEN> ens)
        {
            if (ens == null) return new List<UsuarioViewModel>();

            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
