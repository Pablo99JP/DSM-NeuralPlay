using System;
using System.Collections.Generic;
using ApplicationCore.Domain.EN;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using Xunit;

namespace NeuralPlay.Tests
{
    public class UsuarioAssemblerTests
    {
        [Fact]
        public void ConvertENToViewModel_MapsAllProperties()
        {
            var en = new ApplicationCore.Domain.EN.Usuario
            {
                IdUsuario = 1,
                CorreoElectronico = "a@b.com",
                ContrasenaHash = "pwdhash",
                Nick = "Nombre",
                FechaRegistro = new DateTime(2020, 1, 1)
            };

            var vm = UsuarioAssembler.ConvertENToViewModel(en);

            Assert.NotNull(vm);
            Assert.Equal((int)en.IdUsuario, vm.Id);
            Assert.Equal(en.CorreoElectronico, vm.Email);
            Assert.Equal(en.ContrasenaHash, vm.Password);
            Assert.Equal(en.Nick, vm.Nombre);
            Assert.Equal(string.Empty, vm.Apellidos);
            Assert.Equal(en.FechaRegistro, vm.FechaRegistro);
        }

        [Fact]
        public void ConvertListENToModel_ReturnsSameCountAndMapped()
        {
            var list = new List<ApplicationCore.Domain.EN.Usuario>
            {
                new ApplicationCore.Domain.EN.Usuario { IdUsuario = 1, CorreoElectronico = "x@x.com", ContrasenaHash = "p", Nick = "n", FechaRegistro = DateTime.Now },
                new ApplicationCore.Domain.EN.Usuario { IdUsuario = 2, CorreoElectronico = "y@y.com", ContrasenaHash = "q", Nick = "m", FechaRegistro = DateTime.Now }
            };

            var result = UsuarioAssembler.ConvertListENToModel(list);

            Assert.Equal(list.Count, result.Count);
            Assert.Equal(list[0].CorreoElectronico, result[0].Email);
            Assert.Equal(list[1].CorreoElectronico, result[1].Email);
        }
    }
}
