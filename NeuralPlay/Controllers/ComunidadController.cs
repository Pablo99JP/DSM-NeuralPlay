using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Controllers
{
    public class ComunidadController : BasicController
    {
        private readonly ComunidadCEN _comunidadCEN;
        private readonly IRepository<Comunidad> _comunidadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PublicacionCEN _publicacionCEN;
        private readonly IRepository<Publicacion> _publicacionRepository;

        public ComunidadController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            ComunidadCEN comunidadCEN,
            IRepository<Comunidad> comunidadRepository,
            IUnitOfWork unitOfWork,
            IRepository<Publicacion> publicacionRepository)
            : base(usuarioCEN, usuarioRepository)
        {
            _comunidadCEN = comunidadCEN;
            _comunidadRepository = comunidadRepository;
            _unitOfWork = unitOfWork;
            _publicacionRepository = publicacionRepository;
            _publicacionCEN = new PublicacionCEN(publicacionRepository);
        }

        // GET: /Comunidad
        public IActionResult Index()
        {
            try
            {
                var list = _comunidadCEN.ReadAll_Comunidad();
                var vms = ComunidadAssembler.ConvertListENToViewModel(list);
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<ComunidadViewModel>());
            }
        }

        // GET: /Comunidad/Details/5
        public IActionResult Details(long id)
        {
            try
            {
                var en = _comunidadCEN.ReadOID_Comunidad(id);
                if (en == null) return NotFound();
                var vm = ComunidadAssembler.ConvertENToViewModel(en);
                
                // Cargar los miembros de la comunidad
                var miembros = (en.Miembros ?? Enumerable.Empty<MiembroComunidad>())
                    .Select(m => MiembroComunidadAssembler.ConvertENToViewModel(m))
                    .ToList();
                
                // Cargar las publicaciones de la comunidad
                var publicaciones = (en.Publicaciones ?? Enumerable.Empty<Publicacion>())
                    .Select(p => PublicacionAssembler.ConvertENToViewModel(p))
                    .OrderByDescending(p => p.fechaCreacion)
                    .ToList();
                
                ViewBag.Miembros = (object?)miembros;
                ViewBag.Publicaciones = (object?)publicaciones;
                
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /Comunidad/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Comunidad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ComunidadViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Instanciar explícitamente el CEN y pasar propiedades individuales del ViewModel
                var cen = new ComunidadCEN(_comunidadRepository);

                // Obtener el id del creador de la sesión si está disponible, o usar 1 por defecto
                var idCreador = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

                // En este CEN actual no existe parámetro para el creador; se deja preparado por si se amplía el método.
                // Llamada que NO pasa el ViewModel, solo propiedades primitivas requeridas por el CEN
                cen.NewComunidad(model.Nombre ?? string.Empty, model.Descripcion);

                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al crear la comunidad: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Comunidad/Edit/5
        public IActionResult Edit(long id)
        {
            try
            {
                var en = _comunidadCEN.ReadOID_Comunidad(id);
                if (en == null) return NotFound();
                var vm = ComunidadAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Comunidad/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ComunidadViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var en = _comunidadCEN.ReadOID_Comunidad(model.IdComunidad);
                if (en == null) return NotFound();

                en.Nombre = model.Nombre ?? en.Nombre;
                en.Descripcion = model.Descripcion;
                // FechaCreacion no se modifica normalmente

                _comunidadCEN.ModifyComunidad(en);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: /Comunidad/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            try
            {
                var en = _comunidadCEN.ReadOID_Comunidad(id);
                if (en == null) return NotFound();
                var vm = ComunidadAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Comunidad/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try
            {
                _comunidadCEN.DestroyComunidad(id);
                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                // Redirigimos a la confirmación con un error genérico
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(DeleteConfirm), new { id });
            }
        }
    }
}
