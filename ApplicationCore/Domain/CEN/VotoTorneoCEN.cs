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
        private readonly IUnitOfWork _unitOfWork;

        public VotoTorneoCEN(
            IRepository<VotoTorneo> repo,
            IRepository<PropuestaTorneo> propuestaRepo,
            IMiembroEquipoRepository miembroEquipoRepo,
            IUsuarioRepository usuarioRepo,
            ParticipacionTorneoCEN participacionCEN,
            NotificacionCEN notificacionCEN,
            IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _propuestaRepo = propuestaRepo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _usuarioRepo = usuarioRepo;
            _participacionCEN = participacionCEN;
            _notificacionCEN = notificacionCEN;
            _unitOfWork = unitOfWork;
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

            // Evitar doble voto del mismo usuario: buscar en repositorio por propuesta + usuario
            // (No confiamos solo en la colección por posibles estados previos o lazy loading)
            var votoExistente = _repo.ReadAll().FirstOrDefault(v =>
                v.Propuesta != null && v.Propuesta.IdPropuesta == propuestaId &&
                v.Votante != null && v.Votante.IdUsuario == usuarioId);

            if (votoExistente == null)
            {
                var nuevoVoto = new VotoTorneo
                {
                    Valor = decision,
                    FechaVoto = System.DateTime.UtcNow,
                    Votante = usuario,
                    Propuesta = propuesta
                };
                _repo.New(nuevoVoto);
            }
            else
            {
                // Si ya existe y la decisión es la misma, no hacemos nada
                if (votoExistente.Valor == decision)
                {
                    // Early return: no recalcular lógica si no cambia la decisión
                    _unitOfWork.SaveChanges();
                    return;
                }
                // Actualizamos el voto existente (permite cambiar de opinión antes de unanimidad)
                votoExistente.Valor = decision;
                votoExistente.FechaVoto = System.DateTime.UtcNow;
                _repo.Modify(votoExistente);
            }

            // POSPONER commit hasta después de procesar unanimidad/rechazo.
            // Limpiar duplicados legados (si existían de antes de la corrección)
            var eliminados = LimpiarDuplicados(propuestaId);
            if (eliminados > 0)
            {
                System.Console.WriteLine($"[DEBUG] LimpiarDuplicados: eliminados {eliminados} votos duplicados en propuesta {propuestaId}");
            }
            // Recargar propuesta y votos tras limpieza para asegurar conteo correcto
            propuesta = _propuestaRepo.ReadById(propuestaId) ?? propuesta;
            var votosActualizados = _repo.ReadAll()
                .Where(v => v.Propuesta != null && v.Propuesta.IdPropuesta == propuestaId)
                .ToList();

            // Recuperar miembros del equipo proponente
            var equipo = propuesta.EquipoProponente;
            if (equipo == null) return; // nothing to do

            // Validar que el usuario que intenta votar pertenece al equipo proponente
            var miembrosEquipo = _miembroEquipoRepo.GetUsuariosByEquipo(equipo.IdEquipo).ToList();
            var esMiembro = miembrosEquipo.Any(m => m.IdUsuario == usuarioId);
            if (!esMiembro)
            {
                throw new System.InvalidOperationException("Solo los miembros del equipo proponente pueden votar esta propuesta.");
            }

            var miembros = miembrosEquipo; // reutilizamos la lista ya obtenida

            // Recuperar votos actuales para la propuesta (recargar propuesta desde repo por si acaso)
            propuesta = _propuestaRepo.ReadById(propuestaId) ?? propuesta;
            // Usar lista actualizada (post limpieza) y votos únicos por usuario
            var votosDistinct = votosActualizados
                .Where(v => v.Votante != null)
                .GroupBy(v => v.Votante!.IdUsuario)
                .Select(g => g.First())
                .ToList();

            int totalMiembros = miembros.Count;
            int totalVotos = votosDistinct.Count;

            // Condición de rechazo: al menos un voto negativo
            var anyFalse = votosDistinct.Any(v => v.Valor == false);

            // Condición de aprobación: unanimidad y todos true con votos únicos
            var allTrue = totalVotos > 0 && totalVotos == totalMiembros && votosDistinct.All(v => v.Valor);

            if (allTrue)
            {
                propuesta.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.ACEPTADA;
                _propuestaRepo.Modify(propuesta);

                // Inscribir automáticamente al equipo en el torneo y marcar participación aceptada
                var nuevaPart = _participacionCEN.NewParticipacionTorneo(equipo, propuesta.Torneo!);
                if (nuevaPart != null)
                {
                    nuevaPart.Estado = ApplicationCore.Domain.Enums.EstadoParticipacion.ACEPTADA.ToString();
                    _participacionCEN.ModifyParticipacionTorneo(nuevaPart);
                }

                // Notificar a todos los miembros
                var torneoNombre = propuesta.Torneo?.Nombre ?? "(torneo)";
                foreach (var m in miembros)
                {
                    _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.UNION_TORNEO, $"¡El equipo se ha unido al torneo {torneoNombre}!", m);
                }

                // Guardar todos los cambios (voto + estado + participación + notificaciones)
                _unitOfWork.SaveChanges();
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

                _unitOfWork.SaveChanges();
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
                        _unitOfWork.SaveChanges();
                    }
                }
                // Guardar únicamente el voto en progreso si no se aceptó/rechazó
                if (propuesta.Estado == ApplicationCore.Domain.Enums.EstadoSolicitud.PENDIENTE)
                {
                    _unitOfWork.SaveChanges();
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

        // Limpieza opcional de duplicados (mantiene el primero por fecha)
        public int LimpiarDuplicados(long propuestaId)
        {
            var votos = _repo.ReadAll()
                .Where(v => v.Propuesta != null && v.Propuesta.IdPropuesta == propuestaId)
                .OrderBy(v => v.FechaVoto)
                .ToList();
            var eliminados = 0;
            var vistos = new HashSet<long>();
            foreach (var v in votos)
            {
                var userId = v.Votante?.IdUsuario ?? -1;
                if (userId == -1) continue;
                if (vistos.Contains(userId))
                {
                    _repo.Destroy(v.IdVoto);
                    eliminados++;
                }
                else
                {
                    vistos.Add(userId);
                }
            }
            if (eliminados > 0) _unitOfWork.SaveChanges();
            return eliminados;
        }
    }
}
