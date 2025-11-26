using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using NHibernate;

namespace NeuralPlay.Controllers
{
    public class PublicacionController : BasicController
    {

        private readonly PublicacionCEN _PublicacionCEN;
        private readonly IRepository<Publicacion> _PublicacionRepository;
        private readonly NHibernate.ISession _session;

        public PublicacionController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            IRepository<Publicacion> publicacionRepository,
            NHibernate.ISession session)
            : base(usuarioCEN, usuarioRepository)
        {
            _PublicacionRepository = publicacionRepository;
            _session = session;

            // Instantiate the CEN here using the injected repository so DI container doesn't need a registration for PublicacionCEN
            _PublicacionCEN = new PublicacionCEN(publicacionRepository);
        }


        // GET: PublicacionController
        public ActionResult Index()
        {
            // Use the injected/constructed CEN instead of creating a new NHibernate repository here
            IList<Publicacion> publicaciones = _PublicacionCEN.ReadAll_Publicacion().ToList();

            IEnumerable<PublicacionViewModel> publicacionesVM = PublicacionAssembler.ConvertListENToViewModel(publicaciones).ToList();
                
            return View(publicacionesVM);
        }

        // GET: PublicacionController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var en = _PublicacionCEN.ReadOID_Publicacion(id);
                if (en == null) return NotFound();
                var vm = PublicacionAssembler.ConvertENToViewModel(en);

                // Populate comentarios using assembler helper to ensure non-null list
                vm.comentarios = ComentarioAssembler.ConvertListENToViewModel(en.Comentarios);

                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: PublicacionController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PublicacionController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PublicacionViewModel Publ)
        {
            try
            {
                
                Publicacion newPubl = new Publicacion
                {
                    Contenido = Publ.contenido,
                    FechaCreacion = DateTime.Now,
                    FechaEdicion = DateTime.Now
                };
                _PublicacionCEN.NewPublicacion(newPubl.Contenido);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PublicacionController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                var en = _PublicacionCEN.ReadOID_Publicacion(id);
                if (en == null) return NotFound();

                var vm = PublicacionAssembler.ConvertENToViewModel(en);
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: PublicacionController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PublicacionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var en = _PublicacionCEN.ReadOID_Publicacion(model.idPublicacion);
                if (en == null) return NotFound();

                en.Contenido = model.contenido;
                en.FechaEdicion = DateTime.Now;

                _PublicacionCEN.ModifyPublicacion(en);

                return RedirectToAction(nameof(Index));
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // GET: PublicacionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PublicacionController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
