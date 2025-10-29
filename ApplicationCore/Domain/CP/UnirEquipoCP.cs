using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    public class UnirEquipoCP
    {
        private readonly IRepository<MiembroEquipo> _miembroRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly IRepository<Notificacion> _notRepo;
        private readonly IUnitOfWork _uow;

        public UnirEquipoCP(IRepository<MiembroEquipo> miembroRepo, IUsuarioRepository usuarioRepo, IRepository<Notificacion> notRepo, IUnitOfWork uow)
        {
            _miembroRepo = miembroRepo;
            _usuarioRepo = usuarioRepo;
            _notRepo = notRepo;
            _uow = uow;
        }

        public MiembroEquipo Ejecutar(long usuarioId, Equipo equipo, ApplicationCore.Domain.Enums.RolEquipo rol)
        {
            var usuario = _usuarioRepo.ReadById(usuarioId) ?? throw new System.Exception("Usuario no encontrado");
            var m = new MiembroEquipo { Usuario = usuario, Equipo = equipo, Rol = rol, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = System.DateTime.UtcNow };
            _miembroRepo.New(m);

            var n = new Notificacion { Tipo = ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, Mensaje = $"Usuario {usuario.Nick} unido al equipo {equipo.Nombre}", Leida = false, FechaCreacion = System.DateTime.UtcNow, Destinatario = usuario };
            _notRepo.New(n);

            _uow.SaveChanges();
            return m;
        }
    }
}
