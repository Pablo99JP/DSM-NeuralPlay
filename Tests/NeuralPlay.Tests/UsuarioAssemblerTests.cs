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
            var en = new UsuarioEN
            {
                Id = 1,
                Email = "a@b.com",
                Password = "pwd",
                Nombre = "Nombre",
                Apellidos = "Apellidos",
                FechaRegistro = new DateTime(2020, 1, 1)
            };

            var vm = UsuarioAssembler.ConvertENToViewModel(en);

            Assert.NotNull(vm);
            Assert.Equal(en.Id, vm.Id);
            Assert.Equal(en.Email, vm.Email);
            Assert.Equal(en.Password, vm.Password);
            Assert.Equal(en.Nombre, vm.Nombre);
            Assert.Equal(en.Apellidos, vm.Apellidos);
            Assert.Equal(en.FechaRegistro, vm.FechaRegistro);
        }

        [Fact]
        public void ConvertListENToModel_ReturnsSameCountAndMapped()
        {
            var list = new List<UsuarioEN>
            {
                new UsuarioEN { Id = 1, Email = "x@x.com", Password = "p", Nombre = "n", Apellidos = "a", FechaRegistro = DateTime.Now },
                new UsuarioEN { Id = 2, Email = "y@y.com", Password = "q", Nombre = "m", Apellidos = "b", FechaRegistro = DateTime.Now }
            };

            var result = UsuarioAssembler.ConvertListENToModel(list);

            Assert.Equal(list.Count, result.Count);
            Assert.Equal(list[0].Email, result[0].Email);
            Assert.Equal(list[1].Email, result[1].Email);
        }
    }
}
