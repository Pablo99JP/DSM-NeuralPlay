using System;
using Xunit;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class NHibernateMiembrosTests
    {
        [Fact]
        public void Can_Save_And_Read_MiembroComunidad_And_MiembroEquipo_With_NHibernate()
        {
            using var session = NHibernateHelper.OpenSession();

            var usuarioRepo = new NHibernateUsuarioRepository(session);
            var comunidadRepo = new NHibernateComunidadRepository(session);
            var equipoRepo = new NHibernateEquipoRepository(session);

            var miembroComRepo = new NHibernateMiembroComunidadRepository(session);
            var miembroEqRepo = new NHibernateMiembroEquipoRepository(session);

            using var uow = new NHibernateUnitOfWork(session);

            var usuario = new Usuario { Nick = "UserMiembro", CorreoElectronico = "miembro@example.com" };
            usuarioRepo.New(usuario);

            var comunidad = new Comunidad { Nombre = "ComunidadNH" };
            comunidadRepo.New(comunidad);

            var equipo = new Equipo { Nombre = "EquipoNH" };
            equipoRepo.New(equipo);

            var miembroCom = new MiembroComunidad { Usuario = usuario, Comunidad = comunidad, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow };
            miembroComRepo.New(miembroCom);

            var miembroEq = new MiembroEquipo { Usuario = usuario, Equipo = equipo, Rol = ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow };
            miembroEqRepo.New(miembroEq);

            uow.SaveChanges();

            var loadedCom = miembroComRepo.ReadById(miembroCom.IdMiembroComunidad);
            Assert.NotNull(loadedCom);
            Assert.Equal("UserMiembro", loadedCom!.Usuario.Nick);

            var loadedEq = miembroEqRepo.ReadById(miembroEq.IdMiembroEquipo);
            Assert.NotNull(loadedEq);
            Assert.Equal("UserMiembro", loadedEq!.Usuario.Nick);
        }
    }
}
