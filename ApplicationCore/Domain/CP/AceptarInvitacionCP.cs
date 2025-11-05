using System;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CP
{
    public class AceptarInvitacionCP
    {
        private readonly IRepository<Invitacion> _invitacionRepo;
        private readonly IMiembroEquipoRepository _miembroEquipoRepo;
        private readonly IUnitOfWork _uow;

        public AceptarInvitacionCP(IRepository<Invitacion> invitacionRepo, IMiembroEquipoRepository miembroEquipoRepo, IUnitOfWork uow)
        {
            _invitacionRepo = invitacionRepo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _uow = uow;
        }

        public MiembroEquipo Ejecutar(Invitacion invitacion)
        {
            if (invitacion == null) throw new ArgumentNullException(nameof(invitacion));
            if (invitacion.Tipo != ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO) throw new InvalidOperationException("Invitacion no es de tipo EQUIPO");
            if (invitacion.Destinatario == null) throw new InvalidOperationException("Invitacion sin destinatario");
            if (invitacion.Equipo == null) throw new InvalidOperationException("Invitacion sin equipo");

            var miembro = new MiembroEquipo
            {
                Usuario = invitacion.Destinatario,
                Equipo = invitacion.Equipo,
                Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA,
                FechaAlta = DateTime.UtcNow,
                Rol = ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO,
                FechaAccion = DateTime.UtcNow
            };

            _miembroEquipoRepo.New(miembro);

            invitacion.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
            invitacion.FechaRespuesta = DateTime.UtcNow;
            _invitacionRepo.Modify(invitacion);

            _uow.SaveChanges();

            return miembro;
        }
    }
}
