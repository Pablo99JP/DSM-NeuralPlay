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
 public class InvitacionController : Controller
 {
 private readonly InvitacionCEN _invitacionCEN;
 private readonly IUsuarioRepository _usuarioRepo;
 private readonly IRepository<Comunidad> _comRepo;
 private readonly IRepository<Equipo> _equipoRepo;
 private readonly ApplicationCore.Domain.Repositories.IUnitOfWork _uow;
 private readonly AceptarInvitacionCP _aceptarInvitacionCP;
 private readonly IMiembroComunidadRepository _miembroComunidadRepo;
 private readonly IMiembroEquipoRepository _miembroEquipoRepo;
 private readonly NotificacionCEN _notificacionCEN;
 private readonly MensajeChatCEN _mensajeChatCEN;

 public InvitacionController(InvitacionCEN invitacionCEN, IUsuarioRepository usuarioRepo, IRepository<Comunidad> comRepo, IRepository<Equipo> equipoRepo, ApplicationCore.Domain.Repositories.IUnitOfWork uow, AceptarInvitacionCP aceptarInvitacionCP, IMiembroComunidadRepository miembroComunidadRepo, IMiembroEquipoRepository miembroEquipoRepo, NotificacionCEN notificacionCEN, MensajeChatCEN mensajeChatCEN)
 {
 _invitacionCEN = invitacionCEN;
 _usuarioRepo = usuarioRepo;
 _comRepo = comRepo;
 _equipoRepo = equipoRepo;
 _uow = uow;
 _aceptarInvitacionCP = aceptarInvitacionCP;
 _miembroComunidadRepo = miembroComunidadRepo;
 _miembroEquipoRepo = miembroEquipoRepo;
 _notificacionCEN = notificacionCEN;
 _mensajeChatCEN = mensajeChatCEN;
 }

 public IActionResult Index()
 {
 // Obtener el usuario de sesión
 var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
 if (!usuarioId.HasValue)
 {
 TempData["ErrorMessage"] = "Debes iniciar sesión para ver tus invitaciones.";
 return RedirectToAction("Login", "Usuario");
 }

 // Obtener solo las invitaciones donde el usuario es emisor o receptor
 var invitaciones = _invitacionCEN.ReadAll_Invitacion()
 .Where(inv => 
 (inv.Emisor != null && inv.Emisor.IdUsuario == usuarioId.Value) ||
 (inv.Destinatario != null && inv.Destinatario.IdUsuario == usuarioId.Value)
 )
 .Select(InvitacionAssembler.ConvertENToViewModel)
 .ToList();

 return View(invitaciones);
 }

 public IActionResult Details(long id)
 {
 var en = _invitacionCEN.ReadOID_Invitacion(id);
 if (en == null) return NotFound();
 var vm = InvitacionAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

	[HttpGet]
	public IActionResult Create(long? equipoId = null)
 {
 // Obtener el usuario de sesión como emisor
 var emisorId = HttpContext.Session.GetInt32("UsuarioId");
 if (!emisorId.HasValue)
 {
 TempData["ErrorMessage"] = "Debes iniciar sesión para enviar invitaciones.";
 return RedirectToAction("Login", "Usuario");
 }

 var emisor = _usuarioRepo.ReadById(emisorId.Value);
 if (emisor == null)
 {
 TempData["ErrorMessage"] = "Usuario no encontrado.";
 return RedirectToAction("Index", "Home");
 }

 // Pasar información del emisor
 ViewBag.EmisorId = emisor.IdUsuario;
 ViewBag.EmisorNick = emisor.Nick;

		// Obtener usuarios (excluyendo el emisor)
 ViewBag.Usuarios = _usuarioRepo.ReadAll()
 .Where(u => u.IdUsuario != emisorId.Value)
 .Select(u => new { Id = u.IdUsuario, Name = u.Nick })
 .ToList();

 // Obtener solo las comunidades y equipos donde el emisor es miembro
 var comunidadesEmisor = _miembroComunidadRepo.ReadAll()
 .Where(mc => mc.Usuario != null && mc.Usuario.IdUsuario == emisorId.Value && mc.Comunidad != null)
 .Select(mc => new { Id = mc.Comunidad!.IdComunidad, Name = mc.Comunidad.Nombre })
 .ToList();

 var equiposEmisor = _miembroEquipoRepo.ReadAll()
 .Where(me => me.Usuario != null && me.Usuario.IdUsuario == emisorId.Value && me.Equipo != null)
 .Select(me => new { Id = me.Equipo!.IdEquipo, Name = me.Equipo.Nombre })
 .ToList();

		ViewBag.Comunidades = comunidadesEmisor;
		ViewBag.Equipos = equiposEmisor;

		// Prefill cuando viene desde Equipo/Details
		if (equipoId.HasValue)
		{
			var eq = _equipoRepo.ReadById(equipoId.Value);
			if (eq != null)
			{
				ViewBag.PrefillEquipoId = eq.IdEquipo;
				ViewBag.PrefillEquipoNombre = eq.Nombre;
				ViewBag.PrefillTipo = TipoInvitacion.EQUIPO; // por defecto equipo
				ViewBag.ReturnEquipoId = eq.IdEquipo; // para volver a detalles
			}
		}

 return View();
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
	public IActionResult Create(long destinatarioId, ApplicationCore.Domain.Enums.TipoInvitacion tipo, long? comunidadId, long? equipoId, long? returnEquipoId)
 {
 // Obtener el usuario de sesión como emisor
 var emisorId = HttpContext.Session.GetInt32("UsuarioId");
 if (!emisorId.HasValue)
 {
 TempData["ErrorMessage"] = "Debes iniciar sesión para enviar invitaciones.";
 return RedirectToAction("Login", "Usuario");
 }

 // Validar que el emisor no sea el destinatario
 if (emisorId.Value == destinatarioId)
 {
 ModelState.AddModelError(string.Empty, "No puedes enviarte una invitación a ti mismo.");
 }

 // Validar que el tipo requiera el campo correspondiente
 if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO && !equipoId.HasValue)
 {
 ModelState.AddModelError(string.Empty, "Debes seleccionar un equipo para invitaciones de tipo EQUIPO.");
 }

 if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD && !comunidadId.HasValue)
 {
 ModelState.AddModelError(string.Empty, "Debes seleccionar una comunidad para invitaciones de tipo COMUNIDAD.");
 }

 var emisor = _usuarioRepo.ReadById(emisorId.Value);
 var destinatario = _usuarioRepo.ReadById(destinatarioId);
 if (emisor == null || destinatario == null)
 {
 ModelState.AddModelError(string.Empty, "Emisor o destinatario no válidos.");
 }

 Comunidad? com = null; 
 Equipo? eq = null;
 
 // Validar membresía existente
 if (comunidadId.HasValue)
 {
 com = _comRepo.ReadById(comunidadId.Value);
 // Verificar si el destinatario ya es miembro de la comunidad
 var yaMiembro = _miembroComunidadRepo.ReadAll()
 .Any(mc => mc.Usuario != null && mc.Usuario.IdUsuario == destinatarioId 
 && mc.Comunidad != null && mc.Comunidad.IdComunidad == comunidadId.Value);
 
 if (yaMiembro)
 {
 ModelState.AddModelError(string.Empty, $"El usuario {destinatario?.Nick} ya pertenece a esta comunidad.");
 }
 }
 
 if (equipoId.HasValue)
 {
 eq = _equipoRepo.ReadById(equipoId.Value);
 // Verificar si el destinatario ya es miembro del equipo
 var yaMiembro = _miembroEquipoRepo.ReadAll()
 .Any(me => me.Usuario != null && me.Usuario.IdUsuario == destinatarioId 
 && me.Equipo != null && me.Equipo.IdEquipo == equipoId.Value);
 
 if (yaMiembro)
 {
 ModelState.AddModelError(string.Empty, $"El usuario {destinatario?.Nick} ya pertenece a este equipo.");
 }
 }

	if (!ModelState.IsValid)
 {
 // Re-cargar ViewBag data para la vista
 ViewBag.EmisorId = emisor?.IdUsuario;
 ViewBag.EmisorNick = emisor?.Nick;
 ViewBag.Usuarios = _usuarioRepo.ReadAll()
 .Where(u => u.IdUsuario != emisorId.Value)
 .Select(u => new { Id = u.IdUsuario, Name = u.Nick })
 .ToList();
 
 var comunidadesEmisor = _miembroComunidadRepo.ReadAll()
 .Where(mc => mc.Usuario != null && mc.Usuario.IdUsuario == emisorId.Value && mc.Comunidad != null)
 .Select(mc => new { Id = mc.Comunidad!.IdComunidad, Name = mc.Comunidad.Nombre })
 .ToList();

 var equiposEmisor = _miembroEquipoRepo.ReadAll()
 .Where(me => me.Usuario != null && me.Usuario.IdUsuario == emisorId.Value && me.Equipo != null)
 .Select(me => new { Id = me.Equipo!.IdEquipo, Name = me.Equipo.Nombre })
 .ToList();

			ViewBag.Comunidades = comunidadesEmisor;
			ViewBag.Equipos = equiposEmisor;
			ViewBag.ReturnEquipoId = returnEquipoId;

			// Mantener preselección cuando viene desde Equipo/Details o cuando se indicó en el POST
			if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO)
			{
				var preEqId = equipoId ?? returnEquipoId;
				if (preEqId.HasValue)
				{
					var preEq = _equipoRepo.ReadById(preEqId.Value);
					if (preEq != null)
					{
						ViewBag.PrefillEquipoId = preEq.IdEquipo;
						ViewBag.PrefillEquipoNombre = preEq.Nombre;
						ViewBag.PrefillTipo = ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO;
					}
				}
			}
 
 return View();
 }

 var inv = _invitacionCEN.NewInvitacion(tipo, emisor!, destinatario!, com, eq);
	try 
 { 
 _uow?.SaveChanges();
 TempData["SuccessMessage"] = "Invitación enviada exitosamente.";
 // Notificar al destinatario de la invitación
 var destinoNombre = tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO ? eq?.Nombre : com?.Nombre;
 var mensajeInv = tipo == ApplicationCore.Domain.Enums.TipoInvitacion.EQUIPO
	 ? $"Has recibido una invitación para unirte al equipo '{destinoNombre}'."
	 : $"Has recibido una invitación para unirte a la comunidad '{destinoNombre}'.";
 _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, mensajeInv, destinatario!);
	} 
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = $"Error al enviar la invitación: {ex.Message}";
		if (returnEquipoId.HasValue)
			return RedirectToAction("Create", new { equipoId = returnEquipoId.Value });
		return RedirectToAction(nameof(Create));
 }
 
	// Redirigir a Equipo/Details si procede
	if (equipoId.HasValue)
	{
		return RedirectToAction("Details", "Equipo", new { id = equipoId.Value });
	}
	if (returnEquipoId.HasValue)
	{
		return RedirectToAction("Details", "Equipo", new { id = returnEquipoId.Value });
	}
	return RedirectToAction(nameof(Index));
 }

 [HttpGet]
 public IActionResult Delete(long id)
 {
 var en = _invitacionCEN.ReadOID_Invitacion(id);
 if (en == null) return NotFound();
 var vm = InvitacionAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public IActionResult DeleteConfirmed(long id)
 {
 _invitacionCEN.DestroyInvitacion(id);
 try { _uow?.SaveChanges(); } catch { }
 return RedirectToAction(nameof(Index));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Accept(long id)
 {
 var inv = _invitacionCEN.ReadOID_Invitacion(id);
 if (inv == null) return NotFound();
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
 try
 {
 if (inv.Tipo == TipoInvitacion.EQUIPO)
 {
 // create MiembroEquipo directly instead of CP to avoid CP-specific issues
 if (inv.Destinatario == null || inv.Equipo == null) throw new InvalidOperationException("Invitaci�n inv�lida para equipo.");
 var miembro = new MiembroEquipo
 {
 Usuario = inv.Destinatario,
 Equipo = inv.Equipo,
 Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA,
 FechaAlta = DateTime.UtcNow,
 Rol = ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO,
 FechaAccion = DateTime.UtcNow
 };
 _miembroEquipoRepo.New(miembro);
 inv.Estado = EstadoSolicitud.ACEPTADA;
 inv.FechaRespuesta = DateTime.UtcNow;
 _invitacionCEN.ModifyInvitacion(inv);
 // Notificar al destinatario que se ha unido al equipo
 _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Te has unido al equipo '{inv.Equipo.Nombre}'.", inv.Destinatario);

 // Notificar a los miembros del equipo que hay un nuevo miembro
 var miembrosEquipo = _miembroEquipoRepo.ReadAll()
 .Where(m => m.Equipo != null && m.Equipo.IdEquipo == inv.Equipo.IdEquipo && m.Usuario != null && m.Usuario.IdUsuario != inv.Destinatario.IdUsuario)
 .Select(m => m.Usuario)
 .ToList();

 foreach (var miembroExistente in miembrosEquipo)
 {
 _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Se ha unido un nuevo miembro '{inv.Destinatario.Nick}' a tu equipo '{inv.Equipo.Nombre}'.", miembroExistente);
 }

 // Crear un mensaje en el chat del equipo
 if (inv.Equipo.Chat != null)
 {
 _mensajeChatCEN.NewMensajeChat($"{inv.Destinatario.Nick} se ha unido al equipo.", inv.Destinatario, inv.Equipo.Chat);
 }

 _uow.SaveChanges();
 TempData["SuccessMessage"] = "Invitaci�n aceptada y miembro a�adido al equipo.";
 }
 else if (inv.Tipo == TipoInvitacion.COMUNIDAD)
 {
 if (inv.Destinatario == null || inv.Comunidad == null) throw new InvalidOperationException("Invitaci�n inv�lida para comunidad.");
 var miembro = new MiembroComunidad
 {
 Usuario = inv.Destinatario,
 Comunidad = inv.Comunidad,
 Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA,
 FechaAlta = DateTime.UtcNow,
 Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO,
 FechaAccion = DateTime.UtcNow
 };
 _miembroComunidadRepo.New(miembro);
 inv.Estado = EstadoSolicitud.ACEPTADA;
 inv.FechaRespuesta = DateTime.UtcNow;
 _invitacionCEN.ModifyInvitacion(inv);
 // Notificar al destinatario que se ha unido a la comunidad
 _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Te has unido a la comunidad '{inv.Comunidad.Nombre}'.", inv.Destinatario);
 _uow.SaveChanges();
 TempData["SuccessMessage"] = "Invitaci�n a comunidad aceptada y miembro a�adido.";
 }
 else
 {
 throw new InvalidOperationException("Tipo de invitaci�n no soportado para aceptar.");
 }
 }
 catch (Exception ex)
 {
 TempData["ErrorMessage"] = ex.Message;
 return RedirectToAction(nameof(Details), new { id });
 }
 return RedirectToAction(nameof(Index));
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Reject(long id)
 {
 var inv = _invitacionCEN.ReadOID_Invitacion(id);
 if (inv == null) return NotFound();
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue || inv.Destinatario == null || inv.Destinatario.IdUsuario != uid.Value) return Forbid();
 inv.Estado = ApplicationCore.Domain.Enums.EstadoSolicitud.RECHAZADA;
 inv.FechaRespuesta = DateTime.UtcNow;
 _invitacionCEN.ModifyInvitacion(inv);
 try { _uow?.SaveChanges(); } catch { }
 TempData["SuccessMessage"] = "Invitaci�n rechazada.";
 return RedirectToAction(nameof(Index));
 }
 }
}
