using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using NeuralPlay.Services;
using Infrastructure.NHibernate;
using ApplicationCore.Domain.CP; // Añadido para claridad
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ChatEquipoCEN>();
builder.Services.AddScoped<MensajeChatCEN>(); // REGISTRO AÑADIDO

// Servicios de aplicación
builder.Services.AddScoped<UsuarioCEN>();
builder.Services.AddScoped<PerfilCEN>();
builder.Services.AddScoped<NotificacionCEN>();
builder.Services.AddScoped<ComunidadCEN>();
builder.Services.AddScoped<MiembroComunidadCEN>();
// Registrar CENs relacionados con torneos y votación
builder.Services.AddScoped<PropuestaTorneoCEN>();
builder.Services.AddScoped<VotoTorneoCEN>();
builder.Services.AddScoped<ParticipacionTorneoCEN>();
builder.Services.AddScoped<TorneoCEN>();
builder.Services.AddScoped<MiembroEquipoCEN>();
builder.Services.AddScoped<JuegoCEN>();
builder.Services.AddScoped<EquipoCEN>();

// Register invitation/solicitud CENs used by controllers
builder.Services.AddScoped<ApplicationCore.Domain.CEN.InvitacionCEN>();
builder.Services.AddScoped<ApplicationCore.Domain.CEN.SolicitudIngresoCEN>();

// Register CPs
builder.Services.AddScoped<ApplicationCore.Domain.CP.AceptarInvitacionCP>();

// --- INICIO DE LA CORRECCIÓN ---
// Registrar el servicio de autenticación que faltaba
builder.Services.AddScoped<IUsuarioAuth, UsuarioAuthService>();
// --- FIN DE LA CORRECCIÓN ---

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<ActualizarPerfilCP>();

// Unit of Work (NHibernate)
builder.Services.AddScoped<IUnitOfWork, NHibernateUnitOfWork>();

// Repositorios NHibernate: registros por interfaz/entidad
builder.Services.AddScoped<IUsuarioRepository, NHibernateUsuarioRepository>();
builder.Services.AddScoped<IRepository<Notificacion>, NHibernateNotificacionRepository>();
builder.Services.AddScoped<IRepository<Equipo>, NHibernateEquipoRepository>();
builder.Services.AddScoped<IRepository<Comunidad>, NHibernateComunidadRepository>();
builder.Services.AddScoped<IRepository<Comentario>, NHibernateComentarioRepository>();
builder.Services.AddScoped<IRepository<ChatEquipo>, NHibernateChatEquipoRepository>();
builder.Services.AddScoped<IRepository<Juego>, NHibernateJuegoRepository>();
builder.Services.AddScoped<IRepository<Invitacion>, NHibernateInvitacionRepository>();
builder.Services.AddScoped<IRepository<Sesion>, NHibernateSesionRepository>();
// Register both generic and specific interfaces for Reaccion repository
builder.Services.AddScoped<IRepository<Reaccion>, NHibernateReaccionRepository>();
builder.Services.AddScoped<IReaccionRepository, NHibernateReaccionRepository>();
builder.Services.AddScoped<IRepository<Publicacion>, NHibernatePublicacionRepository>();
builder.Services.AddScoped<IRepository<PropuestaTorneo>, NHibernatePropuestaTorneoRepository>();
builder.Services.AddScoped<IRepository<Perfil>, NHibernatePerfilRepository>();
builder.Services.AddScoped<IRepository<PerfilJuego>, NHibernatePerfilJuegoRepository>();
builder.Services.AddScoped<IParticipacionTorneoRepository, NHibernateParticipacionTorneoRepository>();
builder.Services.AddScoped<IRepository<VotoTorneo>, NHibernateVotoTorneoRepository>();
builder.Services.AddScoped<IMiembroEquipoRepository, NHibernateMiembroEquipoRepository>();
builder.Services.AddScoped<IMiembroComunidadRepository, NHibernateMiembroComunidadRepository>();
builder.Services.AddScoped<IRepository<MensajeChat>, NHibernateMensajeChatRepository>();
builder.Services.AddScoped<IRepository<Torneo>, NHibernateTorneoRepository>();
builder.Services.AddScoped<IRepository<SolicitudIngreso>, NHibernateSolicitudIngresoRepository>();
builder.Services.AddScoped<IRepository<ParticipacionTorneo>, NHibernateParticipacionTorneoRepository>();

// NHibernate session factory and session registration
// SessionFactory is a heavy object and is registered as singleton. ISession is scoped per request.
builder.Services.AddSingleton(NHibernateHelper.SessionFactory);
builder.Services.AddScoped<NHibernate.ISession>(sp => sp.GetRequiredService<NHibernate.ISessionFactory>().OpenSession());

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

// UnitOfWork middleware: commitea los cambios NHibernate al final de cada petición
app.UseMiddleware<NeuralPlay.Middleware.UnitOfWorkMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
