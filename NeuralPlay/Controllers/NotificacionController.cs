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
        private readonly InvitacionCEN? _invitacionCEN;
        private readonly IMiembroComunidadRepository? _miembroComunidadRepo;
        private readonly IMiembroEquipoRepository? _miembroEquipoRepo;
        private readonly MensajeChatCEN? _mensajeChatCEN;
        private readonly ApplicationCore.Domain.Repositories.IUnitOfWork? _uow;

        public NotificacionController(
            UsuarioCEN usuarioCEN, 
            IUsuarioRepository usuarioRepository, 
            NotificacionCEN notificacionCEN, 
            InvitacionCEN invitacionCEN = null, 
            IMiembroComunidadRepository miembroComunidadRepo = null, 
            IMiembroEquipoRepository miembroEquipoRepo = null, 
            MensajeChatCEN mensajeChatCEN = null, 
            ApplicationCore.Domain.Repositories.IUnitOfWork uow = null)
            : base(usuarioCEN, usuarioRepository)
        {
            _notificacionCEN = notificacionCEN;
            _invitacionCEN = invitacionCEN;
            _miembroComunidadRepo = miembroComunidadRepo;
            _miembroEquipoRepo = miembroEquipoRepo;
            _mensajeChatCEN = mensajeChatCEN;
            _uow = uow;
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
            if (_invitacionCEN != null && n.Tipo == TipoNotificacion.SISTEMA && n.Mensaje.Contains("invitación"))
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

            return View(vm);
        }

        // POST: /Notificacion/AcceptInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptInvitation(long invitacionId, long notificacionId)
        {
            if (_invitacionCEN == null || _miembroComunidadRepo == null || _miembroEquipoRepo == null || _mensajeChatCEN == null || _uow == null)
            {
                TempData["ErrorMessage"] = "Servicio no disponible.";
                return RedirectToAction(nameof(Index));
            }

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
            if (_invitacionCEN == null || _uow == null)
            {
                TempData["ErrorMessage"] = "Servicio no disponible.";
                return RedirectToAction(nameof(Index));
            }

            var inv = _invitacionCEN.ReadOID_Invitacion(invitacionId);
            if (inv == null) return NotFound();
            
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
            
            inv.Estado = EstadoSolicitud.RECHAZADA;
            inv.FechaRespuesta = DateTime.UtcNow;
            _invitacionCEN.ModifyInvitacion(inv);
            try { _uow?.SaveChanges(); } catch { }
            
            TempData["SuccessMessage"] = "Invitación rechazada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notificacion/DeleteInvitation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteInvitation(long invitacionId, long notificacionId)
        {
            if (_invitacionCEN == null || _uow == null)
            {
                TempData["ErrorMessage"] = "Servicio no disponible.";
                return RedirectToAction(nameof(Index));
            }

            var inv = _invitacionCEN.ReadOID_Invitacion(invitacionId);
            if (inv == null) return NotFound();
            
            var uid = HttpContext.Session.GetInt32("UsuarioId");
            if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
            
            _invitacionCEN.DestroyInvitacion(invitacionId);
            try { _uow?.SaveChanges(); } catch { }
            
            TempData["SuccessMessage"] = "Invitación eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
