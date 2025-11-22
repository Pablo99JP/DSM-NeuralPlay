using System;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.EN;

Console.WriteLine("Starting NHibernate CRUD runner...");
try
{
    using var session = NHibernateHelper.OpenSession();
    var repo = new Infrastructure.NHibernate.NHibernateUsuarioRepository(session);

    var email = $"cli_test_{Guid.NewGuid():N}@example.com";
    var usuario = new Usuario
    {
        Nick = "CliTest",
        CorreoElectronico = email,
        ContrasenaHash = "hash",
        FechaRegistro = DateTime.UtcNow,
    EstadoCuenta = ApplicationCore.Domain.Enums.EstadoCuenta.ACTIVA
    };

    Console.WriteLine("Creating user...");
    using (var tx = session.BeginTransaction())
    {
        repo.New(usuario);
        tx.Commit();
    }

    Console.WriteLine($"Created user id={usuario.IdUsuario}");

    var read = repo.ReadByEmail(email);
    Console.WriteLine($"Read user: {(read != null ? read.Nick : "null")}");

    if (read != null)
    {
        read.Nick = "CliUpdated";
        using (var tx = session.BeginTransaction())
        {
            repo.Modify(read);
            tx.Commit();
        }
        Console.WriteLine("Updated user nick to CliUpdated");

        var updated = repo.ReadById(read.IdUsuario);
        Console.WriteLine($"After update, nick={updated?.Nick}");

        using (var tx = session.BeginTransaction())
        {
            repo.Destroy(read.IdUsuario);
            tx.Commit();
        }
        Console.WriteLine("Deleted user");

        var after = repo.ReadById(read.IdUsuario);
        Console.WriteLine($"After delete, exists={(after != null)}");
    }
    Console.WriteLine("Done.");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.GetType().Name} - {ex.Message}");
}
