using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Domain.CEN
{
    public class VotoTorneoCEN
    {
        private readonly IRepository<VotoTorneo> _repo;
        private readonly IRepository<PropuestaTorneo> _propuestaRepo;
        private readonly IMiembroEquipoRepository _miembroEquipoRepo;
        private readonly IUsuarioRepository _usuarioRepo;
        private readonly ParticipacionTorneoCEN _participacionCEN;
        private readonly NotificacionCEN _notificacionCEN;

        public VotoTorneoCEN(
            IRepository<VotoTorneo> repo,
            IRepository<PropuestaTorneo> propuestaRepo,
            IMiembroEquipoRepository miembroEquipoRepo,
            IUsuarioRepository usuarioRepo,
            ParticipacionTorneoCEN participacionCEN,
            NotificacionCEN notificacionCEN)
        {
            _repo = repo;
            _propuestaRepo = propuestaRepo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _usuarioRepo = usuarioRepo;
            _participacionCEN = participacionCEN;
            _notificacionCEN = notificacionCEN;
        }

        public VotoTorneo NewVotoTorneo(bool valor, Usuario votante, PropuestaTorneo propuesta)
        {
            var v = new VotoTorneo { Valor = valor, FechaVoto = System.DateTime.UtcNow, Votante = votante, Propuesta = propuesta };
            _repo.New(v);
            return v;
        }

        // EmitirVoto: lógica de árbitro automático que decide aprobar/rechazar propuestas
        public void EmitirVoto(long propuestaId, long usuarioId, bool decision)
        {
            var propuesta = _propuestaRepo.ReadById(propuestaId);
            if (propuesta == null) throw new System.ArgumentException("Propuesta no encontrada", nameof(propuestaId));

            var usuario = _usuarioRepo.ReadById(usuarioId);
            if (usuario == null) throw new System.ArgumentException("Usuario no encontrado", nameof(usuarioId));

            // Evitar doble voto del mismo usuario: si ya votó, actualizar; si no, crear nuevo
            // Intentamos buscar voto existente en la propuesta
            var votoExistente = propuesta.Votos?.FirstOrDefault(v => v.Votante != null && v.Votante.IdUsuario == usuarioId);
            if (votoExistente != null)
            {
                votoExistente.Valor = decision;
                votoExistente.FechaVoto = System.DateTime.UtcNow;
                _repo.Modify(votoExistente);
            }
            else
            {
                var voto = new VotoTorneo { Valor = decision, FechaVoto = System.DateTime.UtcNow, Votante = usuario, Propuesta = propuesta };
                _repo.New(voto);
            }

            // Recuperar miembros del equipo proponente
            var equipo = propuesta.EquipoProponente;
            if (equipo == null) return; // nothing to do

            var miembros = _miembroEquipoRepo.GetUsuariosByEquipo(equipo.IdEquipo).ToList();

            // Recuperar votos actuales para la propuesta (recargar propuesta desde repo por si acaso)
            propuesta = _propuestaRepo.ReadById(propuestaId) ?? propuesta;
            var votos = propuesta.Votos ?? new System.Collections.Generic.List<VotoTorneo>();

            int totalMiembros = miembros.Count;
            int totalVotos = votos.Count;

            // Condición de rechazo: al menos un voto negativo
            var anyFalse = votos.Any(v => v.Valor == false);

            // Condición de aprobación: unanimidad y todos true
            var allTrue = totalVotos > 0 && votos.All(v => v.Valor == true) && totalVotos == totalMiembros;

            if (allTrue)
            {
                propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
                _propuestaRepo.Modify(propuesta);

                // Inscribir automáticamente al equipo en el torneo
                _participacionCEN.NewParticipacionTorneo(equipo, propuesta.Torneo!);

                // Notificar a todos los miembros
                var torneoNombre = propuesta.Torneo?.Nombre ?? "(torneo)";
                foreach (var m in miembros)
                {
                    _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.UNION_TORNEO, $"¡El equipo se ha unido al torneo {torneoNombre}!", m);
                }
            }
            else if (anyFalse)
            {
                propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.RECHAZADA;
                _propuestaRepo.Modify(propuesta);

                var torneoNombre = propuesta.Torneo?.Nombre ?? "(torneo)";
                foreach (var m in miembros)
                {
                    _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.PROPUESTA_TORNEO, $"Propuesta rechazada para el torneo {torneoNombre}", m);
                }
            }
            else
            {
                // No hay unanimidad aún; si existe fecha límite y ha pasado, marcar rechazado
                var fechaLimiteProp = typeof(PropuestaTorneo).GetProperty("FechaLimite");
                if (fechaLimiteProp != null)
                {
                    var fechaLimite = fechaLimiteProp.GetValue(propuesta) as System.DateTime?;
                    if (fechaLimite.HasValue && System.DateTime.UtcNow >= fechaLimite.Value)
                    {
                        propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.RECHAZADA;
                        _propuestaRepo.Modify(propuesta);
                        var torneoNombre = propuesta.Torneo?.Nombre ?? "(torneo)";
                        foreach (var m in miembros)
                        {
                            _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.PROPUESTA_TORNEO, $"Propuesta rechazada para el torneo {torneoNombre}", m);
                        }
                    }
                }
            }

        }

        public VotoTorneo? ReadOID_VotoTorneo(long id) => _repo.ReadById(id);
        public IEnumerable<VotoTorneo> ReadAll_VotoTorneo() => _repo.ReadAll();
        public void ModifyVotoTorneo(VotoTorneo v) => _repo.Modify(v);
        public void DestroyVotoTorneo(long id) => _repo.Destroy(id);
        public IEnumerable<VotoTorneo> BuscarVotosTorneoPorNombreTorneo(string filtro)
        {
            var repoObj = (object)_repo;
            var mi = repoObj.GetType().GetMethod("BuscarVotosTorneoPorNombreTorneo");
            if (mi != null)
            {
                var res = mi.Invoke(repoObj, new object[] { filtro });
                if (res is IEnumerable<VotoTorneo> list) return list;
            }
            return _repo.ReadFilter(filtro);
        }
    }
}
