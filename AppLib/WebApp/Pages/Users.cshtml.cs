using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

public class UsersModel : PageModel
{
    public record UserDto(long Id, string Nick, string Email);

    public List<UserDto> Users { get; private set; } = new();

    public void OnGet()
    {
        var connStr = HttpContext.RequestServices.GetService<Microsoft.Extensions.Configuration.IConfiguration>()?.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr)) connStr = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=ProjectDatabase;TrustServerCertificate=true;";

        try
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            // Try to read users from table 'Usuario' with columns IdUsuario, Nick, CorreoElectronico
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT IdUsuario, Nick, CorreoElectronico FROM Usuario";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.IsDBNull(0) ? 0L : rdr.GetInt64(0);
                var nick = rdr.IsDBNull(1) ? "(sin nick)" : rdr.GetString(1);
                var email = rdr.IsDBNull(2) ? "(sin email)" : rdr.GetString(2);
                Users.Add(new UserDto(id, nick, email));
            }

            if (Users.Count == 0)
            {
                // no rows, fall back to sample
                AddSampleUsers();
            }
        }
        catch (Exception ex)
        {
            // On error (no DB, schema mismatch, etc) fallback to sample users and log
            AddSampleUsers();
            System.Console.WriteLine($"Users page DB read failed: {ex.Message}");
        }
    }

    private void AddSampleUsers()
    {
        Users.Add(new UserDto(1, "alice", "alice@example.com"));
        Users.Add(new UserDto(2, "bob", "bob@example.com"));
        Users.Add(new UserDto(3, "carol", "carol@example.com"));
    }
}
