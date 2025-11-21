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
        /// Convierte la entidad de dominio `ApplicationCore.Domain.EN.Usuario` a `UsuarioViewModel`.
        /// Se eliminó la clase legacy `UsuarioEN` y este assembler trabaja con la entidad de dominio actual.
        /// </summary>
        public static UsuarioViewModel ConvertENToViewModel(ApplicationCore.Domain.EN.Usuario en)
        {
            if (en == null) throw new ArgumentNullException(nameof(en));

            return new UsuarioViewModel
            {
                Id = (int)en.IdUsuario,
                Email = en.CorreoElectronico ?? string.Empty,
                Password = en.ContrasenaHash ?? string.Empty,
                Nombre = en.Nick ?? string.Empty,
                Apellidos = string.Empty, // La entidad dominio actual no tiene apellidos explícitos
                FechaRegistro = en.FechaRegistro
            };
        }

        /// <summary>
        /// Convierte una colección de `ApplicationCore.Domain.EN.Usuario` a una lista de `UsuarioViewModel`.
        /// </summary>
        public static IList<UsuarioViewModel> ConvertListENToModel(IEnumerable<ApplicationCore.Domain.EN.Usuario> ens)
        {
            if (ens == null) return new List<UsuarioViewModel>();

            return ens.Select(ConvertENToViewModel).ToList();
        }
    }
}
