using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using NHibernate;
using System.Linq;

namespace NeuralPlay.Controllers
{
    public class PublicacionController : BasicController
    {

        private readonly PublicacionCEN _PublicacionCEN;
        private readonly IRepository<Publicacion> _PublicacionRepository;
        private readonly NHibernate.ISession _session;
        private readonly IReaccionRepository _reaccionRepository;

        public PublicacionController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            IRepository<Publicacion> publicacionRepository,
            NHibernate.ISession session,
            IReaccionRepository reaccionRepository)
            : base(usuarioCEN, usuarioRepository)
        {
            _PublicacionRepository = publicacionRepository;
            _session = session;
            _reaccionRepository = reaccionRepository;

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

                // Set Like count for publication
                vm.LikeCount = _reaccionRepository.CountByPublicacion(en.IdPublicacion);

                // Determine if current user liked it
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (uid.HasValue)
                {
                    var existing = _reaccionRepository.GetByPublicacionAndAutor(en.IdPublicacion, uid.Value);
                    vm.LikedByUser = existing != null;
                }

                // Populate like counts and liked status for each comment
                if (vm.comentarios != null)
                {
                    foreach (var c in vm.comentarios)
                    {
                        c.LikeCount = _reaccionRepository.CountByComentario(c.idComentario);
                        if (uid.HasValue)
                        {
                            var existingC = _reaccion_repository_GetByComentarioAndAutor_safe(c.idComentario, uid.Value);
                            c.LikedByUser = existingC != null;
                        }
                    }
                }

                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // Helper to avoid naming conflict in generated code - call repository method via interface
        private Reaccion? _reaccion_repository_GetByComentarioAndAutor_safe(long comentarioId, int usuarioId)
        {
            try
            {
                return _reaccionRepository.GetByComentarioAndAutor(comentarioId, usuarioId);
            }
            catch
            {
                return null;
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
