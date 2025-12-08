using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Controllers
{
 public class ComentarioController : Controller
 {
 private readonly ComentarioCEN _comentarioCEN;
 private readonly PublicacionCEN _publicacionCEN;
 private readonly IRepository<Comentario> _comentarioRepo;
 private readonly IRepository<Publicacion> _publicacionRepo;
 private readonly UsuarioCEN _usuarioCEN;

 public ComentarioController(IRepository<Comentario> comentarioRepo, IRepository<Publicacion> publicacionRepo, UsuarioCEN usuarioCEN)
 {
 _comentarioRepo = comentarioRepo;
 _publicacionRepo = publicacionRepo;
 _usuarioCEN = usuarioCEN;

 // Instantiate CENs locally to avoid needing registrations in Program.cs
 _comentarioCEN = new ComentarioCEN(comentarioRepo);
 _publicacionCEN = new PublicacionCEN(publicacionRepo);
 }

 // GET: Comentario/GetCount?publicacionId=5
 [HttpGet]
 public IActionResult GetCount(long publicacionId)
 {
 try
 {
 var pub = _publicacionCEN.ReadOID_Publicacion(publicacionId);
 if (pub == null)
 {
 return Json(new { success = false, error = "Publicación no encontrada" });
 }

 var count = pub.Comentarios?.Count() ?? 0;
 return Json(new { success = true, count = count });
 }
 catch (System.Exception ex)
 {
 return Json(new { success = false, error = ex.Message });
 }
 }

 // GET: Comentario/Create?publicacionId=5
 public IActionResult Create(long publicacionId)
 {
 // Only allow if user is logged in
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue) return Forbid();

 var vm = new ComentarioViewModel { publicacionId = publicacionId, autorId = uid.Value };
 return View(vm);
 }

 // POST: Comentario/Create
 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Create(ComentarioViewModel model)
 {
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue) return Forbid();

 if (!ModelState.IsValid) return View(model);

 var pub = _publicacionCEN.ReadOID_Publicacion(model.publicacionId);
 if (pub == null) return NotFound();

 var autor = _usuarioCEN.ReadOID_Usuario(uid.Value);
 if (autor == null) return NotFound();

 var comentario = _comentarioCEN.NewComentario(model.contenido, autor, pub);

 // Redirect back to Publicacion Details
 return RedirectToAction("Details", "Publicacion", new { id = model.publicacionId });
 }

 // GET: Comentario/Edit/5
 public IActionResult Edit(long id)
 {
 var en = _comentarioCEN.ReadOID_Comentario(id);
 if (en == null) return NotFound();

 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue || en.Autor == null || en.Autor.IdUsuario != uid.Value) return StatusCode(403);

 var vm = ComentarioAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 // POST: Comentario/Edit
 [HttpPost]
 [ValidateAntiForgeryToken]
 public IActionResult Edit(ComentarioViewModel model)
 {
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue) return Forbid();

 if (!ModelState.IsValid) return View(model);
 var en = _comentarioCEN.ReadOID_Comentario(model.idComentario);
 if (en == null) return NotFound();
 if (en.Autor == null || en.Autor.IdUsuario != uid.Value) return StatusCode(403);

 en.Contenido = model.contenido;
 en.FechaEdicion = System.DateTime.UtcNow;
 _comentarioCEN.ModifyComentario(en);

 return RedirectToAction("Details", "Publicacion", new { id = model.publicacionId });
 }

 // GET: Comentario/Delete/5
 public IActionResult Delete(long id)
 {
 var en = _comentarioCEN.ReadOID_Comentario(id);
 if (en == null) return NotFound();
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue || en.Autor == null || en.Autor.IdUsuario != uid.Value) return StatusCode(403);
 var vm = ComentarioAssembler.ConvertENToViewModel(en);
 return View(vm);
 }

 // POST: Comentario/DeleteConfirmed
 [HttpPost, ActionName("Delete")]
 [ValidateAntiForgeryToken]
 public IActionResult DeleteConfirmed(long id)
 {
 var en = _comentarioCEN.ReadOID_Comentario(id);
 if (en == null) return NotFound();
 var uid = HttpContext.Session.GetInt32("UsuarioId");
 if (!uid.HasValue || en.Autor == null || en.Autor.IdUsuario != uid.Value) return StatusCode(403);
 var pubId = en.Publicacion?.IdPublicacion ??0;
 _comentarioCEN.DestroyComentario(id);
 return RedirectToAction("Details", "Publicacion", new { id = pubId });
 }
 }
}
