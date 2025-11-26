using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Controllers
{
    public class EquipoController : BasicController
    {
        private readonly EquipoCEN _equipoCEN;
        private readonly IRepository<Equipo> _equipoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public EquipoController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            EquipoCEN equipoCEN,
            IRepository<Equipo> equipoRepository,
            IUnitOfWork unitOfWork)
            : base(usuarioCEN, usuarioRepository)
        {
            _equipoCEN = equipoCEN;
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
        }

        // GET: /Equipo
        public IActionResult Index()
        {
            try
            {
                var list = _equipoCEN.ReadAll_Equipo();
                var vms = EquipoAssembler.ConvertListENToViewModel(list);
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<EquipoViewModel>());
            }
        }

        // GET: /Equipo/Details/5
        public IActionResult Details(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                
                // Cargar los miembros del equipo
                var miembros = (en.Miembros ?? Enumerable.Empty<MiembroEquipo>())
                    .Select(m => MiembroEquipoAssembler.ConvertENToViewModel(m))
                    .ToList();
                
                ViewBag.Miembros = (object?)miembros;
                
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /Equipo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Equipo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EquipoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var cen = new EquipoCEN(_equipoRepository);
                cen.NewEquipo(model.Nombre ?? string.Empty, model.Descripcion);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear el equipo: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Equipo/Edit/5
        public IActionResult Edit(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Equipo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EquipoViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var en = _equipoCEN.ReadOID_Equipo(model.IdEquipo);
                if (en == null) return NotFound();

                en.Nombre = model.Nombre ?? en.Nombre;
                en.Descripcion = model.Descripcion;
                // FechaCreacion no se modifica normalmente

                _equipoCEN.ModifyEquipo(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Equipo/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            try
            {
                var en = _equipoCEN.ReadOID_Equipo(id);
                if (en == null) return NotFound();
                var vm = EquipoAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Equipo/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try
            {
                _equipoCEN.DestroyEquipo(id);
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
