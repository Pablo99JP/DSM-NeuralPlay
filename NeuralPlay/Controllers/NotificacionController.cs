using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class NotificacionController : BasicController
    {
        private readonly NotificacionCEN _notificacionCEN;
        private readonly InvitacionCEN _invitacionCEN;
        private readonly IMiembroComunidadRepository _miembroComunidadRepo;
        private readonly IMiembroEquipoRepository _miembroEquipoRepo;
        private readonly MensajeChatCEN _mensajeChatCEN;
        private readonly ApplicationCore.Domain.Repositories.IUnitOfWork _uow;
        private readonly SolicitudIngresoCEN _solicitudCEN;

        public NotificacionController(
            UsuarioCEN usuarioCEN, 
            IUsuarioRepository usuarioRepository, 
            NotificacionCEN notificacionCEN, 
            InvitacionCEN invitacionCEN, 
            IMiembroComunidadRepository miembroComunidadRepo, 
            IMiembroEquipoRepository miembroEquipoRepo, 
            MensajeChatCEN mensajeChatCEN, 
            ApplicationCore.Domain.Repositories.IUnitOfWork uow,
            SolicitudIngresoCEN solicitudCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _notificacionCEN = notificacionCEN;
            _invitacionCEN = invitacionCEN;
            _miembroComunidadRepo = miembroComunidadRepo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _mensajeChatCEN = mensajeChatCEN;
            _uow = uow;
            _solicitudCEN = solicitudCEN;
        }

        // GET: /Notificacion
        public IActionResult Index()
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var all = _notificacionCEN.ReadAll_Notificacion();
            var mine = all.Where(n => n.Destinatario != null && n.Destinatario.IdUsuario == userId.Value);
            var vm = NotificacionAssembler.ConvertListENToModel(mine);
            return View(vm);
        }

        // GET: /Notificacion/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

            var vm = NotificacionAssembler.ConvertENToViewModel(n);
            return View(vm);
        }

        // POST: /Notificacion/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

            _notificacionCEN.DestroyNotificacion(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: /Notificacion/Details/5
        public IActionResult Details(long id)
        {
            var userId = HttpContext?.Session?.GetInt32("UsuarioId");
            if (!userId.HasValue) return RedirectToAction("Login", "Usuario");

            var n = _notificacionCEN.ReadOID_Notificacion(id);
            if (n == null) return NotFound();
            if (n.Destinatario == null || n.Destinatario.IdUsuario != userId.Value) return StatusCode(403);

            // Mark notification as read
            if (!n.Leida)
            {
                n.Leida = true;
                _notificacionCEN.ModifyNotificacion(n);
            }

            var vm = NotificacionAssembler.ConvertENToViewModel(n);

            // Si es una notificación de invitación, buscar la invitación pendiente más reciente para este usuario
            if (n.Tipo == TipoNotificacion.SISTEMA && n.Mensaje.Contains("invitación"))
            {
                var invitacionPendiente = _invitacionCEN.ReadAll_Invitacion()
                    .Where(inv => inv.Destinatario != null && inv.Destinatario.IdUsuario == userId.Value
                                  && inv.Estado == EstadoSolicitud.PENDIENTE)
                    .OrderByDescending(inv => inv.FechaEnvio)
                    .FirstOrDefault();

                if (invitacionPendiente != null)
                {
                    ViewBag.Invitacion = InvitacionAssembler.ConvertENToViewModel(invitacionPendiente);
                }
            }

            // Si es una notificación de solicitud de ingreso a equipo, buscar la solicitud y pasar info del equipo
            if (n.Tipo == TipoNotificacion.ALERTA && n.Mensaje.Contains("solicita unirse a tu equipo"))
            {
                // Buscar solicitudes pendientes de equipos donde el usuario actual es admin
                var solicitudesPendientes = _solicitudCEN.ReadAll_SolicitudIngreso()
                    .Where(sol => sol.Estado == EstadoSolicitud.PENDIENTE 
                                  && sol.Tipo == TipoInvitacion.EQUIPO
                                  && sol.Equipo != null
                                  && sol.Equipo.Miembros != null
                                  && sol.Equipo.Miembros.Any(m => m.Rol == RolEquipo.ADMIN && m.Usuario != null && m.Usuario.IdUsuario == userId.Value))
                    .OrderByDescending(sol => sol.FechaSolicitud)
                    .ToList();

                if (solicitudesPendientes.Any())
                {
                    // Encontrar la solicitud que coincide con el mensaje de la notificación
                    var solicitudRelacionada = solicitudesPendientes.FirstOrDefault();
                    if (solicitudRelacionada?.Equipo != null)
                    {
                        ViewBag.SolicitudEquipoId = solicitudRelacionada.Equipo.IdEquipo;
                    }
                }
            }

            return View(vm);
        }

        // POST: /Notificacion/AcceptInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvitation(long invitacionId, long notificacionId)
        {
            var inv = _invitacionCEN.ReadOID_Invitacion(invitacionId);
            if (inv == null) return NotFound();
            
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
            
            try
            {
                if (inv.Tipo == TipoInvitacion.EQUIPO)
                {
                    if (inv.Destinatario == null || inv.Equipo == null) throw new InvalidOperationException("Invitación inválida para equipo.");
                    var miembro = new MiembroEquipo
                    {
                        Usuario = inv.Destinatario,
                        Equipo = inv.Equipo,
                        Estado = EstadoMembresia.ACTIVA,
                        FechaAlta = DateTime.UtcNow,
                        Rol = RolEquipo.MIEMBRO,
                        FechaAccion = DateTime.UtcNow
                    };
                    _miembroEquipoRepo.New(miembro);
                    inv.Estado = EstadoSolicitud.ACEPTADA;
                    inv.FechaRespuesta = DateTime.UtcNow;
                    _invitacionCEN.ModifyInvitacion(inv);
                    
                    _notificacionCEN.NewNotificacion(TipoNotificacion.SISTEMA, $"Te has unido al equipo '{inv.Equipo.Nombre}'.", inv.Destinatario);

                    var miembrosEquipo = _miembroEquipoRepo.ReadAll()
                        .Where(m => m.Equipo != null && m.Equipo.IdEquipo == inv.Equipo.IdEquipo && m.Usuario != null && m.Usuario.IdUsuario != inv.Destinatario.IdUsuario)
                        .Select(m => m.Usuario)
                        .ToList();

                    foreach (var miembroExistente in miembrosEquipo)
                    {
                        _notificacionCEN.NewNotificacion(TipoNotificacion.SISTEMA, $"Se ha unido un nuevo miembro '{inv.Destinatario.Nick}' a tu equipo '{inv.Equipo.Nombre}'.", miembroExistente);
                    }

                    if (inv.Equipo.Chat != null)
                    {
                        _mensajeChatCEN.NewMensajeChat($"{inv.Destinatario.Nick} se ha unido al equipo.", inv.Destinatario, inv.Equipo.Chat);
                    }

                    _uow.SaveChanges();
                    TempData["SuccessMessage"] = "Invitación aceptada y miembro añadido al equipo.";
                }
                else if (inv.Tipo == TipoInvitacion.COMUNIDAD)
                {
                    if (inv.Destinatario == null || inv.Comunidad == null) throw new InvalidOperationException("Invitación inválida para comunidad.");
                    var miembro = new MiembroComunidad
                    {
                        Usuario = inv.Destinatario,
                        Comunidad = inv.Comunidad,
                        Estado = EstadoMembresia.ACTIVA,
                        FechaAlta = DateTime.UtcNow,
                        Rol = RolComunidad.MIEMBRO,
                        FechaAccion = DateTime.UtcNow
                    };
                    _miembroComunidadRepo.New(miembro);
                    inv.Estado = EstadoSolicitud.ACEPTADA;
                    inv.FechaRespuesta = DateTime.UtcNow;
                    _invitacionCEN.ModifyInvitacion(inv);
                    
                    _notificacionCEN.NewNotificacion(TipoNotificacion.SISTEMA, $"Te has unido a la comunidad '{inv.Comunidad.Nombre}'.", inv.Destinatario);
                    _uow.SaveChanges();
                    TempData["SuccessMessage"] = "Invitación a comunidad aceptada y miembro añadido.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notificacion/RejectInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectInvitation(long invitacionId, long notificacionId)
        {
            var inv = _invitacionCEN.ReadOID_Invitacion(invitacionId);
            if (inv == null) return NotFound();
            
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
            
            inv.Estado = EstadoSolicitud.RECHAZADA;
            inv.FechaRespuesta = DateTime.UtcNow;
            _invitacionCEN.ModifyInvitacion(inv);
            try { _uow.SaveChanges(); } catch { }
            
            TempData["SuccessMessage"] = "Invitación rechazada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notificacion/DeleteInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteInvitation(long invitacionId, long notificacionId)
        {
            var inv = _invitacionCEN.ReadOID_Invitacion(invitacionId);
            if (inv == null) return NotFound();
            
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
            
            _invitacionCEN.DestroyInvitacion(invitacionId);
            try { _uow.SaveChanges(); } catch { }
            
            TempData["SuccessMessage"] = "Invitación eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
