using System;
using System.IO;
using System.Linq;
using Infrastructure.NHibernate;
using NHibernate;
using Xunit;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;

namespace Domain.SmokeTests
{
    public class InitializeDbSeedIdempotencyTests
    {
        [Fact]
        public void Seed_Is_Idempotent_On_SQLite_File()
        {
            var tmp = Path.Combine(Path.GetTempPath(), "initdb_seed_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            var sqlitePath = Path.Combine(tmp, "seedtest.db");
            var sqliteConn = $"Data Source={sqlitePath};Version=3;";

            // Export schema to the test sqlite file
            NHibernateHelper.ExportSchema(sqliteConn, "NHibernate.Dialect.SQLiteDialect");

            // Build a SessionFactory for this sqlite file
            var cfg = NHibernateHelper.BuildConfiguration();
            cfg.SetProperty("connection.connection_string", sqliteConn);
            cfg.SetProperty("dialect", "NHibernate.Dialect.SQLiteDialect");
            var sessionFactory = cfg.BuildSessionFactory();

            using (var session = sessionFactory.OpenSession())
            {
                void RunSeed()
                {
                    using var tx = session.BeginTransaction();
                    var usuarioRepo = new NHibernateUsuarioRepository(session);
                    var comunidadRepo = new NHibernateComunidadRepository(session);
                    var equipoRepo = new NHibernateEquipoRepository(session);
                    var miembroComunidadRepo = new NHibernateMiembroComunidadRepository(session);

                    var usuarioCEN = new ApplicationCore.Domain.CEN.UsuarioCEN(usuarioRepo);
                    var comunidadCEN = new ApplicationCore.Domain.CEN.ComunidadCEN(comunidadRepo);
                    var equipoCEN = new ApplicationCore.Domain.CEN.EquipoCEN(equipoRepo);

                    if (usuarioRepo.ReadByNick("alice") == null)
                    {
                        usuarioCEN.NewUsuario("alice", "alice@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("password1"));
                    }
                    if (usuarioRepo.ReadByNick("bob") == null)
                    {
                        usuarioCEN.NewUsuario("bob", "bob@example.com", ApplicationCore.Domain.CEN.PasswordHasher.Hash("password2"));
                    }
                    if (!comunidadRepo.ReadFilter("Gamers").Any())
                    {
                        comunidadCEN.NewComunidad("Gamers", "Comunidad de prueba");
                    }
                    if (!equipoRepo.ReadFilter("TeamA").Any())
                    {
                        equipoCEN.NewEquipo("TeamA", "Equipo de ejemplo");
                    }

                    var existingCom = comunidadRepo.ReadFilter("Gamers").FirstOrDefault();
                    var existingUser = usuarioRepo.ReadByNick("alice");
                    if (existingCom != null && existingUser != null && !miembroComunidadRepo.ReadFilter(existingUser.Nick ?? "").Any())
                    {
                        var mc = new ApplicationCore.Domain.EN.MiembroComunidad { Usuario = existingUser, Comunidad = existingCom, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO };
                        miembroComunidadRepo.New(mc);
                    }

                    tx.Commit();
                }

                // First run
                RunSeed();
                var usuarioRepoCheck = new NHibernateUsuarioRepository(session);
                var comunidadRepoCheck = new NHibernateComunidadRepository(session);
                var miembroRepoCheck = new NHibernateMiembroComunidadRepository(session);

                var uCount1 = usuarioRepoCheck.ReadAll().Count();
                var cCount1 = comunidadRepoCheck.ReadAll().Count();
                var mCount1 = miembroRepoCheck.ReadAll().Count();

                // Second run
                RunSeed();

                var uCount2 = usuarioRepoCheck.ReadAll().Count();
                var cCount2 = comunidadRepoCheck.ReadAll().Count();
                var mCount2 = miembroRepoCheck.ReadAll().Count();

                Assert.Equal(uCount1, uCount2);
                Assert.Equal(cCount1, cCount2);
                Assert.Equal(mCount1, mCount2);
            }
        }
    }
}
