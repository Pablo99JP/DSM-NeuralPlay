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

        public MiembroEquipoController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            MiembroEquipoCEN miembroEquipoCEN,
            IMiembroEquipoRepository miembroEquipoRepository,
            EquipoCEN equipoCEN,
            IRepository<Equipo> equipoRepository,
            IUnitOfWork unitOfWork)
            : base(usuarioCEN, usuarioRepository)
        {
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroEquipoRepository = miembroEquipoRepository;
            _equipoCEN = equipoCEN;
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
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

                en.Rol = model.Rol;
                en.Estado = model.Estado;

                _miembroEquipoCEN.ModifyMiembroEquipo(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
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
                _miembroEquipoCEN.DestroyMiembroEquipo(id);
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
