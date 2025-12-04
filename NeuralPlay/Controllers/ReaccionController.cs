using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NHibernate;

namespace NeuralPlay.Controllers
{
    public class ReaccionController : Controller
    {
        public class TogglePublicacionLikeRequest { public long publicacionId { get; set; } }
        public class ToggleComentarioLikeRequest { public long comentarioId { get; set; } }
        private readonly ReaccionCEN _reaccionCEN;
        private readonly IReaccionRepository _reaccionRepository;
        private readonly UsuarioCEN _usuarioCEN;
        private readonly PublicacionCEN _publicacionCEN;
        private readonly ComentarioCEN _comentarioCEN;
        private readonly IRepository<Reaccion> _reaccionRepo;
        private readonly IRepository<Publicacion> _publicacionRepo;
        private readonly IRepository<Comentario> _comentarioRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly NHibernate.ISession _session;

        public ReaccionController(
            IReaccionRepository reaccionRepository,
            IRepository<Reaccion> reaccionRepo,
            UsuarioCEN usuarioCEN,
            IRepository<Publicacion> publicacionRepo,
            IRepository<Comentario> comentarioRepo,
            IUnitOfWork unitOfWork,
            NHibernate.ISession session)
        {
            _reaccionRepository = reaccionRepository;
            _reaccionRepo = reaccionRepo;
            _usuarioCEN = usuarioCEN;
            _publicacionRepo = publicacionRepo;
            _comentarioRepo = comentarioRepo;
            _unitOfWork = unitOfWork;
            _session = session;

            _reaccionCEN = new ReaccionCEN(reaccionRepo);
            _publicacionCEN = new PublicacionCEN(publicacionRepo);
            _comentarioCEN = new ComentarioCEN(comentarioRepo);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult TogglePublicacionLike([FromBody] TogglePublicacionLikeRequest request)
        {
            try
            {
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue) return Unauthorized();

                var autor = _usuarioCEN.ReadOID_Usuario(uid.Value);
                if (autor == null) return NotFound("Usuario no encontrado");

            var publicacion = _publicacionCEN.ReadOID_Publicacion(request.publicacionId);
                if (publicacion == null) return NotFound("Publicación no encontrada");

                // Check if user already liked this publication
            var existing = _reaccionRepository.GetByPublicacionAndAutor(request.publicacionId, uid.Value);

                bool liked;
                
                if (existing != null)
                {
                    // Unlike: just remove from collection (cascade will handle delete)
                    Console.WriteLine($"Removing like {existing.IdReaccion} from publication {request.publicacionId}");
                    publicacion.Reacciones.Remove(existing);
                    liked = false;
                }
                else
                {
                    // Like: create new reaction and add to collection
                    Console.WriteLine($"Adding like to publication {request.publicacionId} by user {uid.Value}");
                    var nuevaReaccion = new Reaccion 
                    { 
                        Tipo = TipoReaccion.ME_GUSTA, 
                        FechaCreacion = DateTime.UtcNow, 
                        Autor = autor, 
                        Publicacion = publicacion
                    };
                    publicacion.Reacciones.Add(nuevaReaccion);
                    liked = true;
                }
                
                // Save changes
                _unitOfWork.SaveChanges();
                
                // Query count after changes are committed
                var likeCount = _reaccionRepository.CountByPublicacion(request.publicacionId);

                Console.WriteLine($"Toggle result: liked={liked}, count={likeCount}");
                return Json(new { success = true, likeCount = likeCount, liked = liked });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TogglePublicacionLike error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ToggleComentarioLike([FromBody] ToggleComentarioLikeRequest request)
        {
            try
            {
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue) return Unauthorized();

                var autor = _usuarioCEN.ReadOID_Usuario(uid.Value);
                if (autor == null) return NotFound("Usuario no encontrado");

                var comentario = _comentarioCEN.ReadOID_Comentario(request.comentarioId);
                if (comentario == null) return NotFound("Comentario no encontrado");

                // Check if user already liked this comment
                var existing = _reaccionRepository.GetByComentarioAndAutor(request.comentarioId, uid.Value);

                bool liked;
                
                if (existing != null)
                {
                    // Unlike: just remove from collection (cascade will handle delete)
                    Console.WriteLine($"Removing like {existing.IdReaccion} from comment {request.comentarioId}");
                    comentario.Reacciones.Remove(existing);
                    liked = false;
                }
                else
                {
                    // Like: create new reaction and add to collection
                    Console.WriteLine($"Adding like to comment {request.comentarioId} by user {uid.Value}");
                    var nuevaReaccion = new Reaccion 
                    { 
                        Tipo = TipoReaccion.ME_GUSTA, 
                        FechaCreacion = DateTime.UtcNow, 
                        Autor = autor, 
                        Comentario = comentario
                    };
                    comentario.Reacciones.Add(nuevaReaccion);
                    liked = true;
                }
                
                // Save changes
                _unitOfWork.SaveChanges();
                
                // Query count after changes are committed
                var likeCount = _reaccionRepository.CountByComentario(request.comentarioId);

                Console.WriteLine($"Toggle result: liked={liked}, count={likeCount}");
                return Json(new { success = true, likeCount = likeCount, liked = liked });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToggleComentarioLike error: {ex}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
