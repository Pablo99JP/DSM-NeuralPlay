using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernatePerfilJuegoTests
    {
        [Fact]
        public void Can_Save_And_Read_PerfilJuego_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var perfilRepo = new NHibernatePerfilRepository(session);
            var juegoRepo = new NHibernateJuegoRepository(session);
            var perfilJuegoRepo = new NHibernatePerfilJuegoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "PGUser", CorreoElectronico = "pg@example.com" };
            usuarioRepo.New(usuario);

            var perfil = new Perfil { Usuario = usuario, FotoPerfilUrl = null, Descripcion = "Gamer", VisibilidadPerfil = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO, VisibilidadActividad = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO };
            perfilRepo.New(perfil);

            var juego = new Juego { NombreJuego = "JuegoPerfil", Genero = ApplicationCore.Domain.Enums.GeneroJuego.RPG };
            juegoRepo.New(juego);

            var perfilJuego = new PerfilJuego { Perfil = perfil, Juego = juego, FechaAdicion = DateTime.UtcNow };
            perfilJuegoRepo.New(perfilJuego);

            uow.SaveChanges();

            var loaded = perfilJuegoRepo.ReadById(perfilJuego.IdPerfilJuego);
            Assert.NotNull(loaded);
            Assert.Equal("JuegoPerfil", loaded!.Juego.NombreJuego);
        }
    }
}
