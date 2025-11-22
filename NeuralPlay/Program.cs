using ApplicationCore.Domain.Repositories;
using ApplicationCore.Infrastructure.Memory;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using NeuralPlay.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUsuarioRepository, InMemoryUsuarioRepository>();
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<IUsuarioAuth, UsuarioAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
// Registrar repositorio y CEN para Notificacion (in-memory)
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Notificacion>, ApplicationCore.Infrastructure.Memory.InMemoryRepository<Notificacion>>();
builder.Services.AddScoped<ApplicationCore.Domain.CEN.NotificacionCEN>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware debe añadirse tras UseRouting y antes de UseAuthorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
