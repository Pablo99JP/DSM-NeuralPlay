using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class MiembroEquipoController : BasicController
    {
        private readonly MiembroEquipoCEN _miembroEquipoCEN;
        private readonly IMiembroEquipoRepository _miembroEquipoRepository;
        private readonly EquipoCEN _equipoCEN;
        private readonly IRepository<Equipo> _equipoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly NotificacionCEN _notificacionCEN;

        public MiembroEquipoController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            MiembroEquipoCEN miembroEquipoCEN,
            IMiembroEquipoRepository miembroEquipoRepository,
            EquipoCEN equipoCEN,
            IRepository<Equipo> equipoRepository,
            IUnitOfWork unitOfWork,
            NotificacionCEN notificacionCEN)
            : base(usuarioCEN, usuarioRepository)
        {
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroEquipoRepository = miembroEquipoRepository;
            _equipoCEN = equipoCEN;
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
            _notificacionCEN = notificacionCEN;
        }

        // GET: /MiembroEquipo
        public IActionResult Index()
        {
            try
            {
                var list = _miembroEquipoCEN.ReadAll_MiembroEquipo();
                var vms = MiembroEquipoAssembler.ConvertListENToViewModel(list);
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<MiembroEquipoViewModel>());
            }
        }

        // GET: /MiembroEquipo/Details/5
        public IActionResult Details(long id)
        {
            try
            {
                var en = _miembroEquipoCEN.ReadOID_MiembroEquipo(id);
                if (en == null) return NotFound();
                var vm = MiembroEquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /MiembroEquipo/Create
        public IActionResult Create()
        {
            try
            {
                // Cargar lista de usuarios
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");

                // Cargar lista de equipos
                var equipos = _equipoCEN.ReadAll_Equipo();
                ViewBag.Equipos = new SelectList(equipos, "IdEquipo", "Nombre");

                // Cargar roles disponibles
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));

                return View();
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroEquipo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MiembroEquipoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar las listas en caso de error
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");
                var equipos = _equipoCEN.ReadAll_Equipo();
                ViewBag.Equipos = new SelectList(equipos, "IdEquipo", "Nombre");
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));
                return View(model);
            }

            try
            {
                // Obtener las entidades relacionadas
                var usuario = _usuarioCEN.ReadOID_Usuario(model.IdUsuario);
                if (usuario == null)
                {
                    ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                    return View(model);
                }

                var equipo = _equipoCEN.ReadOID_Equipo(model.IdEquipo);
                if (equipo == null)
                {
                    ModelState.AddModelError(string.Empty, "Equipo no encontrado.");
                    return View(model);
                }

                // Crear el miembro usando el CEN
                var cen = new MiembroEquipoCEN(_miembroEquipoRepository);
                cen.NewMiembroEquipo(usuario, equipo, model.Rol);

                // Notificar al usuario que se ha unido al equipo
                _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Te has unido al equipo '{equipo.Nombre}'.", usuario);

                // Notificar a los miembros del equipo que hay un nuevo miembro
                var miembrosEquipo = _miembroEquipoRepository.ReadAll()
                    .Where(m => m.Equipo != null && m.Equipo.IdEquipo == equipo.IdEquipo && m.Usuario != null && m.Usuario.IdUsuario != usuario.IdUsuario)
                    .Select(m => m.Usuario)
                    .ToList();

                foreach (var miembro in miembrosEquipo)
                {
                    _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Se ha unido un nuevo miembro '{usuario.Nick}' a tu equipo '{equipo.Nombre}'.", miembro);
                }

                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el miembro: {ex.Message}");
                // Recargar las listas
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");
                var equipos = _equipoCEN.ReadAll_Equipo();
                ViewBag.Equipos = new SelectList(equipos, "IdEquipo", "Nombre");
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));
                return View(model);
            }
        }

        // GET: /MiembroEquipo/Edit/5
        public IActionResult Edit(long id)
        {
            try
            {
                var en = _miembroEquipoCEN.ReadOID_MiembroEquipo(id);
                if (en == null) return NotFound();
                var vm = MiembroEquipoAssembler.ConvertENToViewModel(en);

                // Cargar roles y estados disponibles
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));

                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroEquipo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MiembroEquipoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));
                return View(model);
            }

            try
            {
                var en = _miembroEquipoCEN.ReadOID_MiembroEquipo(model.IdMiembroEquipo);
                if (en == null) return NotFound();

                var oldRol = en.Rol;
                var oldEstado = en.Estado;

                // Si se está asignando el rol de ADMIN a este usuario
                if (model.Rol == RolEquipo.ADMIN && oldRol != RolEquipo.ADMIN)
                {
                    // Buscar el admin actual del equipo
                    var adminActual = _miembroEquipoRepository
                        .ReadAll()
                        .FirstOrDefault(m => m.Equipo.IdEquipo == en.Equipo.IdEquipo && m.Rol == RolEquipo.ADMIN && m.IdMiembroEquipo != en.IdMiembroEquipo);

                    // Si existe un admin actual, cambiar su rol a MIEMBRO
                    if (adminActual != null)
                    {
                        adminActual.Rol = RolEquipo.MIEMBRO;
                        _miembroEquipoCEN.ModifyMiembroEquipo(adminActual);

                        // Notificar al admin anterior
                        if (adminActual.Usuario != null && adminActual.Equipo != null)
                        {
                            _notificacionCEN.NewNotificacion(
                                ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA,
                                $"Tu rol de administrador en el equipo '{adminActual.Equipo.Nombre}' ha sido transferido a otro miembro.",
                                adminActual.Usuario);
                        }
                    }
                }

                en.Rol = model.Rol;
                en.Estado = model.Estado;

                _miembroEquipoCEN.ModifyMiembroEquipo(en);

                // Notificaciones por cambios
                if (en.Usuario != null && en.Equipo != null)
                {
                    if (oldRol != en.Rol)
                    {
                        _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Tu rol en el equipo '{en.Equipo.Nombre}' ha cambiado a {en.Rol}.", en.Usuario);
                    }

                    if (oldEstado != en.Estado)
                    {
                        if (en.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA)
                        {
                            _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Has sido expulsado del equipo '{en.Equipo.Nombre}'.", en.Usuario);
                        }
                        else if (en.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA && oldEstado != ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                        {
                            _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Te has unido al equipo '{en.Equipo.Nombre}'.", en.Usuario);
                        }
                    }
                }

                try { _unitOfWork?.SaveChanges(); } catch { }
                var equipoId = en.Equipo?.IdEquipo ?? model.IdEquipo;
                return RedirectToAction("Details", "Equipo", new { id = equipoId });
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolEquipo)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));
                return View(model);
            }
        }

        // GET: /MiembroEquipo/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            try
            {
                var en = _miembroEquipoCEN.ReadOID_MiembroEquipo(id);
                if (en == null) return NotFound();
                var vm = MiembroEquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroEquipo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try
            {
                var en = _miembroEquipoCEN.ReadOID_MiembroEquipo(id);
                var equipoId = en?.Equipo?.IdEquipo;

                if (en != null && en.Usuario != null && en.Equipo != null)
                {
                    _notificacionCEN.NewNotificacion(ApplicationCore.Domain.Enums.TipoNotificacion.SISTEMA, $"Has sido expulsado del equipo '{en.Equipo.Nombre}'.", en.Usuario);
                }

                _miembroEquipoCEN.DestroyMiembroEquipo(id);
                try { _unitOfWork?.SaveChanges(); } catch { }
                if (equipoId.HasValue)
                {
                    return RedirectToAction("Details", "Equipo", new { id = equipoId.Value });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(DeleteConfirm), new { id });
            }
        }
    }
}
