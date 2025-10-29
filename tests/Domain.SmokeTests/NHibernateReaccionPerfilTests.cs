using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateReaccionPerfilTests
    {
        [Fact]
        public void Can_Save_And_Read_Reaccion_And_Perfil_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var publicacionRepo = new NHibernatePublicacionRepository(session);
            var reaccionRepo = new NHibernateReaccionRepository(session);
            var perfilRepo = new NHibernatePerfilRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "ReacUser", CorreoElectronico = "reac@example.com" };
            usuarioRepo.New(usuario);

            var publicacion = new Publicacion { Contenido = "Contenido prueba", FechaCreacion = DateTime.UtcNow };
            publicacionRepo.New(publicacion);

            var reaccion = new Reaccion { Autor = usuario, Publicacion = publicacion, Tipo = ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, FechaCreacion = DateTime.UtcNow };
            reaccionRepo.New(reaccion);

            var perfil = new Perfil { Usuario = usuario, FotoPerfilUrl = "http://img", Descripcion = "Bio", VisibilidadPerfil = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO, VisibilidadActividad = ApplicationCore.Domain.Enums.Visibilidad.PUBLICO };
            perfilRepo.New(perfil);

            uow.SaveChanges();

            var loadedR = reaccionRepo.ReadById(reaccion.IdReaccion);
            Assert.NotNull(loadedR);
            Assert.Equal(ApplicationCore.Domain.Enums.TipoReaccion.ME_GUSTA, loadedR!.Tipo);

            var loadedP = perfilRepo.ReadById(perfil.IdPerfil);
            Assert.NotNull(loadedP);
            Assert.Equal("Bio", loadedP!.Descripcion);
        }
    }
}
