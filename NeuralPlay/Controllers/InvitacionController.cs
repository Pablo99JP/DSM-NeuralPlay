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

 public InvitacionController(InvitacionCEN invitacionCEN, IUsuarioRepository usuarioRepo, IRepository<Comunidad> comRepo, IRepository<Equipo> equipoRepo, ApplicationCore.Domain.Repositories.IUnitOfWork uow, AceptarInvitacionCP aceptarInvitacionCP, IMiembroComunidadRepository miembroComunidadRepo, IMiembroEquipoRepository miembroEquipoRepo)
 {
 _invitacionCEN = invitacionCEN;
 _usuarioRepo = usuarioRepo;
 _comRepo = comRepo;
 _equipoRepo = equipoRepo;
 _uow = uow;
 _aceptarInvitacionCP = aceptarInvitacionCP;
 _miembroComunidadRepo = miembroComunidadRepo;
 _miembroEquipoRepo = miembroEquipoRepo;
 }

 public IActionResult Index()
 {
 var list = _invitacionCEN.ReadAll_Invitacion().Select(InvitacionAssembler.ConvertENToViewModel).ToList();
 return View(list);
 }

 public IActionResult Details(long id)
 {
 var en = _invitacionCEN.ReadOID_Invitacion(id);
 if (en == null) return NotFound();
 var vm = InvitacionAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 [HttpGet]
 public IActionResult Create()
 {
 ViewBag.Usuarios = _usuarioRepo.ReadAll().Select(u => new { Id = u.IdUsuario, Name = u.Nick }).ToList();
 ViewBag.Comunidades = _comRepo.ReadAll().Select(c => new { Id = c.IdComunidad, Name = c.Nombre }).ToList();
 ViewBag.Equipos = _equipoRepo.ReadAll().Select(e => new { Id = e.IdEquipo, Name = e.Nombre }).ToList();
 return View();
 }

 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Create(long emisorId, long destinatarioId, ApplicationCore.Domain.Enums.TipoInvitacion tipo, long? comunidadId, long? equipoId)
 {
 // server-side validation
 if (emisorId == destinatarioId)
 {
 ModelState.AddModelError(string.Empty, "El emisor y destinatario no pueden ser el mismo usuario.");
 }
 var emisor = _usuarioRepo.ReadById(emisorId);
 var destinatario = _usuarioRepo.ReadById(destinatarioId);
 if (emisor == null || destinatario == null)
 {
 ModelState.AddModelError(string.Empty, "Emisor o destinatario no válidos.");
 }
 Comunidad? com = null; Equipo? eq = null;
 if (comunidadId.HasValue) com = _comRepo.ReadById(comunidadId.Value);
 if (equipoId.HasValue) eq = _equipoRepo.ReadById(equipoId.Value);
 if (!ModelState.IsValid)
 {
 ViewBag.Usuarios = _usuarioRepo.ReadAll().Select(u => new { Id = u.IdUsuario, Name = u.Nick }).ToList();
 ViewBag.Comunidades = _comRepo.ReadAll().Select(c => new { Id = c.IdComunidad, Name = c.Nombre }).ToList();
 ViewBag.Equipos = _equipoRepo.ReadAll().Select(e => new { Id = e.IdEquipo, Name = e.Nombre }).ToList();
 return View();
 }
 var inv = _invitacionCEN.NewInvitacion(tipo, emisor!, destinatario!, com, eq);
 try { _uow?.SaveChanges(); } catch { }
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
 if (inv.Destinatario == null || inv.Equipo == null) throw new InvalidOperationException("Invitación inválida para equipo.");
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
 Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA,
 FechaAlta = DateTime.UtcNow,
 Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO,
 FechaAccion = DateTime.UtcNow
 };
 _miembroComunidadRepo.New(miembro);
 inv.Estado = EstadoSolicitud.ACEPTADA;
 inv.FechaRespuesta = DateTime.UtcNow;
 _invitacionCEN.ModifyInvitacion(inv);
 _uow.SaveChanges();
 TempData["SuccessMessage"] = "Invitación a comunidad aceptada y miembro añadido.";
 }
 else
 {
 throw new InvalidOperationException("Tipo de invitación no soportado para aceptar.");
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
 TempData["SuccessMessage"] = "Invitación rechazada.";
 return RedirectToAction(nameof(Index));
 }
 }
}
