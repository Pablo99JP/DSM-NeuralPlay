using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Load a default connection string (matches NHibernate.cfg.xml). You can override in appsettings.json
builder.Configuration.AddInMemoryCollection(new[] {
    new KeyValuePair<string,string?>("ConnectionStrings:DefaultConnection","Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Database=ProjectDatabase;TrustServerCertificate=true;")
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
