using System;
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
        /// Convierte una entidad UsuarioEN (la entidad simple creada para el módulo) a UsuarioViewModel mapeando propiedad por propiedad.
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

        /// <summary>
        /// Sobrecarga: convierte la entidad de dominio existente `ApplicationCore.Domain.EN.Usuario` a `UsuarioViewModel`.
        /// Esto permite integrar el Assembler con la CEN y Repositorios ya existentes en la solución.
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
