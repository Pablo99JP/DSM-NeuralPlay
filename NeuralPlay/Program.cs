using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using NeuralPlay.Services;
using Infrastructure.NHibernate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Servicios de aplicación
builder.Services.AddScoped<UsuarioCEN>();
// Registrar CENs mínimos necesarios
builder.Services.AddScoped<NotificacionCEN>();
// Registrar CEN para Publicacion y otras entidades que usamos en los controladores
builder.Services.AddScoped<PublicacionCEN>();
builder.Services.AddScoped<ComentarioCEN>();
builder.Services.AddScoped<ReaccionCEN>();
builder.Services.AddScoped<InvitacionCEN>();
builder.Services.AddScoped<SolicitudIngresoCEN>();
// Registrar CENs relacionados con torneos y votación
builder.Services.AddScoped<PropuestaTorneoCEN>();
builder.Services.AddScoped<VotoTorneoCEN>();
builder.Services.AddScoped<ParticipacionTorneoCEN>();
builder.Services.AddScoped<MiembroEquipoCEN>();
builder.Services.AddScoped<JuegoCEN>();
builder.Services.AddScoped<EquipoCEN>();
builder.Services.AddScoped<IUsuarioAuth, UsuarioAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

// Unit of Work (NHibernate)
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IUnitOfWork, Infrastructure.NHibernate.NHibernateUnitOfWork>();

// Repositorios NHibernate: registros por interfaz/entidad
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IUsuarioRepository, Infrastructure.NHibernate.NHibernateUsuarioRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Notificacion>, Infrastructure.NHibernate.NHibernateNotificacionRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Equipo>, Infrastructure.NHibernate.NHibernateEquipoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Comunidad>, Infrastructure.NHibernate.NHibernateComunidadRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Comentario>, Infrastructure.NHibernate.NHibernateComentarioRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<ChatEquipo>, Infrastructure.NHibernate.NHibernateChatEquipoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Juego>, Infrastructure.NHibernate.NHibernateJuegoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Invitacion>, Infrastructure.NHibernate.NHibernateInvitacionRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Sesion>, Infrastructure.NHibernate.NHibernateSesionRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Reaccion>, Infrastructure.NHibernate.NHibernateReaccionRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Publicacion>, Infrastructure.NHibernate.NHibernatePublicacionRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<PropuestaTorneo>, Infrastructure.NHibernate.NHibernatePropuestaTorneoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Perfil>, Infrastructure.NHibernate.NHibernatePerfilRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<PerfilJuego>, Infrastructure.NHibernate.NHibernatePerfilJuegoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IParticipacionTorneoRepository, Infrastructure.NHibernate.NHibernateParticipacionTorneoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<VotoTorneo>, Infrastructure.NHibernate.NHibernateVotoTorneoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IMiembroEquipoRepository, Infrastructure.NHibernate.NHibernateMiembroEquipoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IMiembroComunidadRepository, Infrastructure.NHibernate.NHibernateMiembroComunidadRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<MensajeChat>, Infrastructure.NHibernate.NHibernateMensajeChatRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<Torneo>, Infrastructure.NHibernate.NHibernateTorneoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<SolicitudIngreso>, Infrastructure.NHibernate.NHibernateSolicitudIngresoRepository>();
builder.Services.AddScoped<ApplicationCore.Domain.Repositories.IRepository<ParticipacionTorneo>, Infrastructure.NHibernate.NHibernateParticipacionTorneoRepository>();

// NHibernate session factory and session registration
// SessionFactory is a heavy object and is registered as singleton. ISession is scoped per request.
builder.Services.AddSingleton(Infrastructure.NHibernate.NHibernateHelper.SessionFactory);
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
