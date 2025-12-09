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
    public class MiembroComunidadController : BasicController
    {
        private readonly MiembroComunidadCEN _miembroComunidadCEN;
        private readonly IMiembroComunidadRepository _miembroComunidadRepository;
        private readonly ComunidadCEN _comunidadCEN;
        private readonly IRepository<Comunidad> _comunidadRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MiembroComunidadController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            MiembroComunidadCEN miembroComunidadCEN,
            IMiembroComunidadRepository miembroComunidadRepository,
            ComunidadCEN comunidadCEN,
            IRepository<Comunidad> comunidadRepository,
            IUnitOfWork unitOfWork)
            : base(usuarioCEN, usuarioRepository)
        {
            _miembroComunidadCEN = miembroComunidadCEN;
            _miembroComunidadRepository = miembroComunidadRepository;
            _comunidadCEN = comunidadCEN;
            _comunidadRepository = comunidadRepository;
            _unitOfWork = unitOfWork;
        }

        // GET: /MiembroComunidad
        public IActionResult Index()
        {
            try
            {
                var list = _miembroComunidadCEN.ReadAll_MiembroComunidad();
                var vms = MiembroComunidadAssembler.ConvertListENToViewModel(list);
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<MiembroComunidadViewModel>());
            }
        }

        // GET: /MiembroComunidad/Details/5
        public IActionResult Details(long id)
        {
            try
            {
                var en = _miembroComunidadCEN.ReadOID_MiembroComunidad(id);
                if (en == null) return NotFound();
                var vm = MiembroComunidadAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /MiembroComunidad/Detalles?idComunidad=X
        public IActionResult Detalles(long idComunidad)
        {
            try
            {
                // Verificar que el usuario actual es líder de la comunidad
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return RedirectToAction("Login", "Usuario");
                }

                var comunidad = _comunidadCEN.ReadOID_Comunidad(idComunidad);
                if (comunidad == null) return NotFound();

                // Verificar que el usuario actual tiene permisos (es LIDER)
                var miembroActual = comunidad.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value);
                if (miembroActual == null || miembroActual.Rol != RolComunidad.LIDER)
                {
                    return Forbid();
                }

                // Obtener todos los miembros de la comunidad
                var miembros = (comunidad.Miembros ?? Enumerable.Empty<MiembroComunidad>())
                    .Select(m => MiembroComunidadAssembler.ConvertENToViewModel(m))
                    .OrderBy(m => m.Rol)
                    .ThenBy(m => m.NombreUsuario)
                    .ToList();

                var comunidadVM = ComunidadAssembler.ConvertENToViewModel(comunidad);
                ViewBag.Miembros = (object?)miembros;
                ViewBag.Roles = System.Enum.GetValues(typeof(RolComunidad)).Cast<RolComunidad>().ToList();

                return View(comunidadVM);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /MiembroComunidad/Create
        public IActionResult Create()
        {
            try
            {
                // Cargar lista de usuarios
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");

                // Cargar lista de comunidades
                var comunidades = _comunidadCEN.ReadAll_Comunidad();
                ViewBag.Comunidades = new SelectList(comunidades, "IdComunidad", "Nombre");

                // Cargar roles disponibles
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));

                return View();
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroComunidad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MiembroComunidadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Recargar las listas en caso de error
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");
                var comunidades = _comunidadCEN.ReadAll_Comunidad();
                ViewBag.Comunidades = new SelectList(comunidades, "IdComunidad", "Nombre");
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));
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

                var comunidad = _comunidadCEN.ReadOID_Comunidad(model.IdComunidad);
                if (comunidad == null)
                {
                    ModelState.AddModelError(string.Empty, "Comunidad no encontrada.");
                    return View(model);
                }

                // Crear el miembro usando el CEN
                var cen = new MiembroComunidadCEN(_miembroComunidadRepository);
                cen.NewMiembroComunidad(usuario, comunidad, model.Rol);

                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el miembro: {ex.Message}");
                // Recargar las listas
                var usuarios = _usuarioCEN.ReadAll_Usuario();
                ViewBag.Usuarios = new SelectList(usuarios, "IdUsuario", "Nick");
                var comunidades = _comunidadCEN.ReadAll_Comunidad();
                ViewBag.Comunidades = new SelectList(comunidades, "IdComunidad", "Nombre");
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));
                return View(model);
            }
        }

        // GET: /MiembroComunidad/Edit/5
        public IActionResult Edit(long id)
        {
            try
            {
                var en = _miembroComunidadCEN.ReadOID_MiembroComunidad(id);
                if (en == null) return NotFound();
                var vm = MiembroComunidadAssembler.ConvertENToViewModel(en);

                // Cargar roles disponibles
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));

                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroComunidad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(MiembroComunidadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));
                return View(model);
            }

            try
            {
                var en = _miembroComunidadCEN.ReadOID_MiembroComunidad(model.IdMiembroComunidad);
                if (en == null) return NotFound();

                en.Rol = model.Rol;
                en.Estado = model.Estado;

                _miembroComunidadCEN.ModifyMiembroComunidad(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Roles = new SelectList(System.Enum.GetValues(typeof(RolComunidad)));
                ViewBag.Estados = new SelectList(System.Enum.GetValues(typeof(EstadoMembresia)));
                return View(model);
            }
        }

        // POST: /MiembroComunidad/CambiarRol
        [HttpPost]
        public IActionResult CambiarRol(long miembroId, RolComunidad nuevoRol)
        {
            try
            {
                // Verificar autenticación
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return Json(new { success = false, message = "No autenticado" });
                }

                // Obtener el miembro a modificar
                var miembro = _miembroComunidadCEN.ReadOID_MiembroComunidad(miembroId);
                if (miembro == null)
                {
                    return Json(new { success = false, message = "Miembro no encontrado" });
                }

                // Verificar que el usuario actual es líder de la comunidad
                var comunidad = miembro.Comunidad;
                var miembroActual = comunidad.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value);
                if (miembroActual == null || miembroActual.Rol != RolComunidad.LIDER)
                {
                    return Json(new { success = false, message = "No tienes permisos para realizar esta acción" });
                }

                // No permitir que el líder cambie su propio rol
                if (miembro.IdMiembroComunidad == miembroActual.IdMiembroComunidad)
                {
                    return Json(new { success = false, message = "No puedes cambiar tu propio rol" });
                }

                // Cambiar el rol
                _miembroComunidadCEN.CambiarRol(miembro, nuevoRol);
                _unitOfWork?.SaveChanges();

                return Json(new { success = true, message = "Rol actualizado correctamente" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /MiembroComunidad/ExpulsarMiembro
        [HttpPost]
        public IActionResult ExpulsarMiembro(long miembroId)
        {
            try
            {
                // Verificar autenticación
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return Json(new { success = false, message = "No autenticado" });
                }

                // Obtener el miembro a expulsar
                var miembro = _miembroComunidadCEN.ReadOID_MiembroComunidad(miembroId);
                if (miembro == null)
                {
                    return Json(new { success = false, message = "Miembro no encontrado" });
                }

                // Verificar que el usuario actual es líder de la comunidad
                var comunidad = miembro.Comunidad;
                var miembroActual = comunidad.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value);
                if (miembroActual == null || miembroActual.Rol != RolComunidad.LIDER)
                {
                    return Json(new { success = false, message = "No tienes permisos para realizar esta acción" });
                }

                // No permitir que el líder se expulse a sí mismo
                if (miembro.IdMiembroComunidad == miembroActual.IdMiembroComunidad)
                {
                    return Json(new { success = false, message = "No puedes expulsarte a ti mismo" });
                }

                // Expulsar al miembro
                _miembroComunidadCEN.Expulsar(miembro);
                _unitOfWork?.SaveChanges();

                return Json(new { success = true, message = "Miembro expulsado correctamente" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /MiembroComunidad/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            try
            {
                var en = _miembroComunidadCEN.ReadOID_MiembroComunidad(id);
                if (en == null) return NotFound();
                var vm = MiembroComunidadAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /MiembroComunidad/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try
            {
                _miembroComunidadCEN.DestroyMiembroComunidad(id);
                try { _unitOfWork?.SaveChanges(); } catch { }
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
