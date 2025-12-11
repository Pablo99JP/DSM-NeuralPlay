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

 public SolicitudIngresoController(SolicitudIngresoCEN solCEN, IUsuarioRepository usuarioRepo, IRepository<Comunidad> comRepo, IRepository<Equipo> equipoRepo, ApplicationCore.Domain.Repositories.IUnitOfWork uow, IMiembroEquipoRepository miembroEquipoRepo, IMiembroComunidadRepository miembroComunidadRepo, MiembroComunidadCEN miembroComunidadCEN)
 {
 _solCEN = solCEN;
 _usuarioRepo = usuarioRepo;
 _comRepo = comRepo;
 _equipoRepo = equipoRepo;
 _uow = uow;
 _miembroEquipoRepo = miembroEquipoRepo;
 _miembroComunidadRepo = miembroComunidadRepo;
 _miembroComunidadCEN = miembroComunidadCEN;
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
 
 try { _uow?.SaveChanges(); } catch {}
 
 // Redirigir según el tipo de solicitud
 if (tipo == ApplicationCore.Domain.Enums.TipoInvitacion.COMUNIDAD && comunidadId.HasValue)
 {
 return RedirectToAction("Details", "Comunidad", new { id = comunidadId.Value });
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
 // simplistic permission: allow any logged user (should restrict to community leader / team admins)
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue) return Forbid();
 if (s.Estado != EstadoSolicitud.PENDIENTE) return BadRequest("Solicitud ya resuelta");

 if (s.Equipo != null)
 {
 var miembro = new MiembroEquipo { Usuario = s.Solicitante!, Equipo = s.Equipo, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolEquipo.MIEMBRO, FechaAccion = DateTime.UtcNow };
 _miembroEquipoRepo.New(miembro);
 }
 else if (s.Comunidad != null)
 {
 var miembro = new MiembroComunidad { Usuario = s.Solicitante!, Comunidad = s.Comunidad, Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA, FechaAlta = DateTime.UtcNow, Rol = ApplicationCore.Domain.Enums.RolComunidad.MIEMBRO, FechaAccion = DateTime.UtcNow };
 _miembroComunidadRepo.New(miembro);
 }
 s.Estado = EstadoSolicitud.ACEPTADA;
 s.FechaResolucion = DateTime.UtcNow;
 _solCEN.ModifySolicitudIngreso(s);
 try { _uow?.SaveChanges(); } catch {}
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
 if (s.Estado != EstadoSolicitud.PENDIENTE) return BadRequest("Solicitud ya resuelta");
 s.Estado = EstadoSolicitud.RECHAZADA;
 s.FechaResolucion = DateTime.UtcNow;
 _solCEN.ModifySolicitudIngreso(s);
 try { _uow?.SaveChanges(); } catch {}
 return RedirectToAction(nameof(Index));
 }
 }
}
