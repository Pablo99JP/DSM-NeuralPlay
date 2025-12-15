using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ApplicationCore.Domain.CEN;
using NeuralPlay.Models;
using NeuralPlay.Assemblers;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Enums;
using System;

namespace NeuralPlay.Controllers
{
 public class SolicitudIngresoController : Controller
 {
 private readonly SolicitudIngresoCEN _solCEN;
 private readonly IUsuarioRepository _usuarioRepo;
 private readonly IRepository<Comunidad> _comRepo;
 private readonly IRepository<Equipo> _equipoRepo;
 private readonly ApplicationCore.Domain.Repositories.IUnitOfWork _uow;
 private readonly IMiembroEquipoRepository _miembroEquipoRepo;
 private readonly IMiembroComunidadRepository _miembroComunidadRepo;
 private readonly MiembroComunidadCEN _miembroComunidadCEN;
 private readonly NotificacionCEN _notificacionCEN;
 private readonly MensajeChatCEN _mensajeChatCEN;
 private readonly IRepository<ChatEquipo> _chatEquipoRepo;

 public SolicitudIngresoController(SolicitudIngresoCEN solCEN, IUsuarioRepository usuarioRepo, IRepository<Comunidad> comRepo, IRepository<Equipo> equipoRepo, ApplicationCore.Domain.Repositories.IUnitOfWork uow, IMiembroEquipoRepository miembroEquipoRepo, IMiembroComunidadRepository miembroComunidadRepo, MiembroComunidadCEN miembroComunidadCEN, NotificacionCEN notificacionCEN, MensajeChatCEN mensajeChatCEN, IRepository<ChatEquipo> chatEquipoRepo)
 {
 _solCEN = solCEN;
 _usuarioRepo = usuarioRepo;
 _comRepo = comRepo;
 _equipoRepo = equipoRepo;
 _uow = uow;
 _miembroEquipoRepo = miembroEquipoRepo;
 _miembroComunidadRepo = miembroComunidadRepo;
 _miembroComunidadCEN = miembroComunidadCEN;
 _notificacionCEN = notificacionCEN;
 _mensajeChatCEN = mensajeChatCEN;
 _chatEquipoRepo = chatEquipoRepo;
 }

 public IActionResult Index()
 {
 var list = _solCEN.ReadAll_SolicitudIngreso().Select(SolicitudIngresoAssembler.ConvertENToViewModel).ToList();
 return View(list);
 }

 public IActionResult Details(long id)
 {
 var en = _solCEN.ReadOID_SolicitudIngreso(id);
 if (en == null) return NotFound();
 var vm = SolicitudIngresoAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 [HttpGet]
 public IActionResult Create(long? comunidadId = null, long? equipoId = null)
 {
 // Obtener el usuario actual de la sesión
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue) return RedirectToAction("Login", "Usuario");
 
 var usuario = _usuarioRepo.ReadById(uid.Value);
 if (usuario == null) return RedirectToAction("Login", "Usuario");

 ViewBag.Usuarios = _usuarioRepo.ReadAll().Select(u => new { Id = u.IdUsuario, Name = u.Nick }).ToList();
 ViewBag.Comunidades = _comRepo.ReadAll().Select(c => new { Id = c.IdComunidad, Name = c.Nombre }).ToList();
 ViewBag.Equipos = _equipoRepo.ReadAll().Select(e => new { Id = e.IdEquipo, Name = e.Nombre }).ToList();
 
 // Pasar los parámetros a la vista
 ViewBag.UsuarioActualId = uid.Value;
 ViewBag.ComunidadIdPreseleccionada = comunidadId;
 ViewBag.EquipoIdPreseleccionado = equipoId;
 
 // Determinar el tipo según el parámetro
 if (comunidadId.HasValue)
 {
 ViewBag.TipoPreseleccionado = 0; // COMUNIDAD
 }
 else if (equipoId.HasValue)
 {
 ViewBag.TipoPreseleccionado = 1; // EQUIPO
 }
 
 return View();
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Create(long solicitanteId, ApplicationCore.Domain.Enums.TipoInvitacion tipo, long? comunidadId, long? equipoId)
 {
 // Validación: debe haber uno y solo uno de los dos (comunidad o equipo)
 if ((!comunidadId.HasValue && !equipoId.HasValue) || (comunidadId.HasValue && equipoId.HasValue))
 {
 ModelState.AddModelError(string.Empty, "Debes seleccionar una comunidad O un equipo, no ambos ni ninguno.");
 ViewBag.Usuarios = _usuarioRepo.ReadAll().Select(u => new { Id = u.IdUsuario, Name = u.Nick }).ToList();
 ViewBag.Comunidades = _comRepo.ReadAll().Select(c => new { Id = c.IdComunidad, Name = c.Nombre }).ToList();
 ViewBag.Equipos = _equipoRepo.ReadAll().Select(e => new { Id = e.IdEquipo, Name = e.Nombre }).ToList();
 ViewBag.UsuarioActualId = solicitanteId;
 ViewBag.ComunidadIdPreseleccionada = comunidadId;
 ViewBag.EquipoIdPreseleccionado = equipoId;
 ViewBag.HuboError = true;
 if (comunidadId.HasValue) ViewBag.TipoPreseleccionado = 0;
 else if (equipoId.HasValue) ViewBag.TipoPreseleccionado = 1;
 return View();
 }
 
 var solicitante = _usuarioRepo.ReadById(solicitanteId);
 Comunidad? com = null; Equipo? eq = null;
 if (comunidadId.HasValue) com = _comRepo.ReadById(comunidadId.Value);
 if (equipoId.HasValue) eq = _equipoRepo.ReadById(equipoId.Value);
 try
 {
 var s = _solCEN.NewSolicitudIngreso(tipo, solicitante!, com, eq);
 
 // Si es una solicitud de comunidad, se aprueba automáticamente y se crea el miembro
 if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD && com != null)
 {
 // Usar el CEN para crear el miembro, aplicando todas las validaciones
 _miembroComunidadCEN.NewMiembroComunidad(solicitante!, com, ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO);
 }
 else if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO && eq != null)
 {
 // Para equipos: enviar notificación al admin del equipo
 var miembroAdmin = eq.Miembros?.FirstOrDefault(m => m.Rol == ApplicationCore.Domain.Enums.RolEquipo.ADMIN);
 if (miembroAdmin?.Usuario != null)
 {
 var mensaje = $"El usuario {solicitante?.Nick} solicita unirse a tu equipo {eq.Nombre}";
 _notificacionCEN.NewNotificacion(TipoNotificacion.ALERTA, mensaje, miembroAdmin.Usuario);
 }
 
 // Crear mensaje en el chat del equipo con formato especial para identificar solicitudes
 if (eq.Chat != null)
 {
 var mensajeChat = $"[SOLICITUD_INGRESO:{s.IdSolicitud}] El usuario {solicitante?.Nick} ha solicitado unirse al equipo.";
 _mensajeChatCEN.NewMensajeChat(mensajeChat, solicitante!, eq.Chat);
 }
 }
 
 try { _uow?.SaveChanges(); } catch {}
 
 // Redirigir según el tipo de solicitud
 if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD && comunidadId.HasValue)
 {
 TempData["SuccessMessage"] = "Te has unido a la comunidad correctamente.";
 return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
 }
 else if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO && equipoId.HasValue)
 {
 TempData["SuccessMessage"] = "Tu solicitud de ingreso al equipo ha sido enviada. El administrador del equipo la revisará pronto.";
 return RedirectToAction("Index", "Home", new { tab = "equipos" });
 }
 }
 catch (Exception ex)
 {
 ModelState.AddModelError(string.Empty, ex.Message);
 ViewBag.Usuarios = _usuarioRepo.ReadAll().Select(u => new { Id = u.IdUsuario, Name = u.Nick }).ToList();
 ViewBag.Comunidades = _comRepo.ReadAll().Select(c => new { Id = c.IdComunidad, Name = c.Nombre }).ToList();
 ViewBag.Equipos = _equipoRepo.ReadAll().Select(e => new { Id = e.IdEquipo, Name = e.Nombre }).ToList();
 ViewBag.UsuarioActualId = solicitanteId;
 ViewBag.ComunidadIdPreseleccionada = comunidadId;
 ViewBag.EquipoIdPreseleccionado = equipoId;
 ViewBag.HuboError = true;
 if (comunidadId.HasValue) ViewBag.TipoPreseleccionado = 0;
 else if (equipoId.HasValue) ViewBag.TipoPreseleccionado = 1;
 return View();
 }
 return RedirectToAction(nameof(Index));
 }

 [HttpGet]
 public IActionResult Delete(long id)
 {
 var en = _solCEN.ReadOID_SolicitudIngreso(id);
 if (en == null) return NotFound();
 var vm = SolicitudIngresoAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public IActionResult DeleteConfirmed(long id)
 {
 _solCEN.DestroySolicitudIngreso(id);
 try { _uow?.SaveChanges(); } catch {}
 return RedirectToAction(nameof(Index));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Accept(long id)
 {
     var s = _solCEN.ReadOID_SolicitudIngreso(id);
     if (s == null) return NotFound();
     
     var uid = HttpContext.Session.GetInt32("UsuarioId");
     if (!uid.HasValue) return Forbid();
     
     // Validar que el usuario actual es admin del equipo (si es solicitud de equipo)
     if (s.Equipo != null)
     {
         var miembroAdmin = s.Equipo.Miembros?.FirstOrDefault(m => m.Rol == ApplicationCore.Domain.Enums.RolEquipo.ADMIN && m.Usuario?.IdUsuario == uid.Value);
         if (miembroAdmin == null) return Forbid("Solo el admin del equipo puede aceptar solicitudes");
     }

     try
     {
         if (s.Estado == EstadoSolicitud.PENDIENTE)
         {
             if (s.Equipo != null)
             {
                 // Verificar si ya es miembro para evitar duplicados
                 var yaEsMiembro = s.Equipo.Miembros?.Any(m => m.Usuario?.IdUsuario == s.Solicitante?.IdUsuario) ?? false;
                 if (!yaEsMiembro)
                 {
                     var miembro = new MiembroEquipo { Usuario = s.Solicitante!, Equipo = s.Equipo, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO, FechaAccion = DateTime.UtcNow };
                     _miembroEquipoRepo.New(miembro);
                 }
             }
             else if (s.Comunidad != null)
             {
                 var yaEsMiembro = s.Comunidad.Miembros?.Any(m => m.Usuario?.IdUsuario == s.Solicitante?.IdUsuario) ?? false;
                 if (!yaEsMiembro)
                 {
                     var miembro = new MiembroComunidad { Usuario = s.Solicitante!, Comunidad = s.Comunidad, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO, FechaAccion = DateTime.UtcNow };
                     _miembroComunidadRepo.New(miembro);
                 }
             }
             s.Estado = EstadoSolicitud.ACEPTADA;
             s.FechaResolucion = DateTime.UtcNow;
             _solCEN.ModifySolicitudIngreso(s);
         }

         // Eliminar el mensaje de solicitud del chat si existe (incluso si ya estaba aceptada)
         if (s.Equipo != null && s.Equipo.Chat != null)
         {
             var mensajeSolicitud = s.Equipo.Chat.Mensajes?.FirstOrDefault(m => 
                 m.Contenido != null && m.Contenido.StartsWith($"[SOLICITUD_INGRESO:{s.IdSolicitud}]"));
             if (mensajeSolicitud != null)
             {
                 s.Equipo.Chat.Mensajes?.Remove(mensajeSolicitud);
                 _mensajeChatCEN.DestroyMensajeChat(mensajeSolicitud.IdMensajeChat);
             }
         }
         
         _uow?.SaveChanges();
     }
     catch (Exception ex)
     {
         TempData["Error"] = "Error al procesar la solicitud: " + ex.Message;
     }
     
     // Si es una solicitud de equipo, redirigir al chat del equipo
     if (s.Equipo != null)
     {
         return RedirectToAction("Chat", "Equipo", new { id = s.Equipo.IdEquipo });
     }
     return RedirectToAction(nameof(Index));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Reject(long id)
 {
     var s = _solCEN.ReadOID_SolicitudIngreso(id);
     if (s == null) return NotFound();
     var uid = HttpContext.Session.GetInt32("UsuarioId");
     if (!uid.HasValue) return Forbid();
     
     // Validar que el usuario actual es admin del equipo (si es solicitud de equipo)
     if (s.Equipo != null)
     {
         var miembroAdmin = s.Equipo.Miembros?.FirstOrDefault(m => m.Rol == ApplicationCore.Domain.Enums.RolEquipo.ADMIN && m.Usuario?.IdUsuario == uid.Value);
         if (miembroAdmin == null) return Forbid("Solo el admin del equipo puede rechazar solicitudes");
     }
     
     try
     {
         if (s.Estado == EstadoSolicitud.PENDIENTE)
         {
             s.Estado = EstadoSolicitud.RECHAZADA;
             s.FechaResolucion = DateTime.UtcNow;
             _solCEN.ModifySolicitudIngreso(s);
         }
         
         // Eliminar el mensaje de solicitud del chat si existe
         if (s.Equipo != null && s.Equipo.Chat != null)
         {
             var mensajeSolicitud = s.Equipo.Chat.Mensajes?.FirstOrDefault(m => 
                 m.Contenido != null && m.Contenido.StartsWith($"[SOLICITUD_INGRESO:{s.IdSolicitud}]"));
             if (mensajeSolicitud != null)
             {
                 s.Equipo.Chat.Mensajes?.Remove(mensajeSolicitud);
                 _mensajeChatCEN.DestroyMensajeChat(mensajeSolicitud.IdMensajeChat);
             }
         }
         
         _uow?.SaveChanges();
     }
     catch (Exception ex)
     {
         TempData["Error"] = "Error al rechazar la solicitud: " + ex.Message;
     }
     
     // Si es una solicitud de equipo, redirigir al chat del equipo
     if (s.Equipo != null)
     {
         return RedirectToAction("Chat", "Equipo", new { id = s.Equipo.IdEquipo });
     }
     return RedirectToAction(nameof(Index));
 }
 }
}
