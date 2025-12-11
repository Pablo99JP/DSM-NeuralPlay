using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace NeuralPlay.Controllers
{
    public class ComunidadController : BasicController
    {
        private readonly ComunidadCEN _comunidadCEN;
        private readonly IRepository<Comunidad> _comunidadRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PublicacionCEN _publicacionCEN;
        private readonly IRepository<Publicacion> _publicacionRepository;
        private readonly IReaccionRepository _reaccionRepository;
        private readonly IMiembroComunidadRepository _miembroComunidadRepository;
        private readonly NeuralPlay.Services.IUsuarioAuth _usuarioAuth;
        private readonly MiembroComunidadCEN _miembroComunidadCEN;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ComunidadController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            ComunidadCEN comunidadCEN,
            IRepository<Comunidad> comunidadRepository,
            IUnitOfWork unitOfWork,
            IRepository<Publicacion> publicacionRepository,
            IReaccionRepository reaccionRepository,
            IMiembroComunidadRepository miembroComunidadRepository,
            NeuralPlay.Services.IUsuarioAuth usuarioAuth,
            MiembroComunidadCEN miembroComunidadCEN,
            IWebHostEnvironment webHostEnvironment)
            : base(usuarioCEN, usuarioRepository)
        {
            _comunidadCEN = comunidadCEN;
            _comunidadRepository = comunidadRepository;
            _unitOfWork = unitOfWork;
            _publicacionRepository = publicacionRepository;
            _reaccionRepository = reaccionRepository;
            _publicacionCEN = new PublicacionCEN(publicacionRepository);
            _miembroComunidadRepository = miembroComunidadRepository;
            _usuarioAuth = usuarioAuth;
            _miembroComunidadCEN = miembroComunidadCEN;
            _webHostEnvironment = webHostEnvironment;
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

        // GET: /Comunidad/MisComunidades
        public IActionResult MisComunidades()
        {
            try
            {
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return RedirectToAction("Login", "Usuario");
                }

                // Obtener todas las membresías activas del usuario
                var miembros = _miembroComunidadRepository.ReadAll()
                    .Where(m => m.Usuario != null && m.Usuario.IdUsuario == uid.Value 
                             && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                    .ToList();

                // Extraer las comunidades
                var comunidades = miembros
                    .Where(m => m.Comunidad != null)
                    .Select(m => ComunidadAssembler.ConvertENToViewModel(m.Comunidad!))
                    .ToList();

                return View(comunidades);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(new List<ComunidadViewModel>());
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
                
                // Cargar los miembros de la comunidad (solo activos)
                var miembros = (en.Miembros ?? Enumerable.Empty<MiembroComunidad>())
                    .Where(m => m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                    .Select(m => MiembroComunidadAssembler.ConvertENToViewModel(m))
                    .ToList();
                
                // Obtener el usuario actual
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                
                // Determinar el rol del usuario en esta comunidad
                string rolUsuario = "Jugador"; // Valor por defecto
                if (uid.HasValue)
                {
                    var miembroActual = en.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value 
                        && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                    if (miembroActual != null)
                    {
                        rolUsuario = miembroActual.Rol.ToString();
                    }
                }
                
                // Cargar las publicaciones de la comunidad
                var publicaciones = (en.Publicaciones ?? Enumerable.Empty<Publicacion>())
                    .Select(p => {
                        var pubVM = PublicacionAssembler.ConvertENToViewModel(p);
                        // Cargar comentarios
                        if (pubVM != null)
                        {
                            pubVM.comentarios = ComentarioAssembler.ConvertListENToViewModel(p.Comentarios);
                            
                            // Cargar like count
                            pubVM.LikeCount = _reaccionRepository.CountByPublicacion(p.IdPublicacion);
                            
                            // Verificar si el usuario actual dio like
                            if (uid.HasValue)
                            {
                                var existing = _reaccionRepository.GetByPublicacionAndAutor(p.IdPublicacion, uid.Value);
                                pubVM.LikedByUser = existing != null;
                            }
                        }
                        return pubVM;
                    })
                    .OrderByDescending(p => p.fechaCreacion)
                    .ToList();
                
                ViewBag.Miembros = (object?)miembros;
                ViewBag.Publicaciones = (object?)publicaciones;
                ViewBag.RolUsuario = rolUsuario;
                
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
        public async Task<IActionResult> Create(ComunidadViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Obtener el usuario actual
                var usuarioActual = _usuarioAuth.GetUsuarioActual();
                if (usuarioActual == null)
                {
                    ModelState.AddModelError(string.Empty, "No se pudo obtener el usuario actual.");
                    return View(model);
                }

                // Procesar la imagen si fue subida
                string? rutaImagen = null;
                if (model.ImagenArchivo != null && model.ImagenArchivo.Length > 0)
                {
                    // Validar tipo de archivo
                    var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string extension = Path.GetExtension(model.ImagenArchivo.FileName).ToLowerInvariant();
                    
                    if (!extensionesPermitidas.Contains(extension))
                    {
                        ModelState.AddModelError("ImagenArchivo", "Solo se permiten imágenes (jpg, jpeg, png, gif, webp).");
                        return View(model);
                    }
                    
                    // Generar nombre único para el archivo
                    string nombreArchivo = $"{Guid.NewGuid()}{extension}";
                    
                    // Definir ruta de guardado
                    string directorioComunidades = Path.Combine(_webHostEnvironment.WebRootPath, "Recursos", "Comunidades");
                    
                    // Crear directorio si no existe
                    if (!Directory.Exists(directorioComunidades))
                    {
                        Directory.CreateDirectory(directorioComunidades);
                    }
                    
                    string rutaCompleta = Path.Combine(directorioComunidades, nombreArchivo);
                    
                    // Guardar el archivo
                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await model.ImagenArchivo.CopyToAsync(stream);
                    }
                    
                    // Actualizar la ruta relativa para guardar en BD
                    rutaImagen = $"/Recursos/Comunidades/{nombreArchivo}";
                }

                // Crear la comunidad
                var cen = new ComunidadCEN(_comunidadRepository);
                var nuevaComunidad = cen.NewComunidad(model.Nombre ?? string.Empty, model.Descripcion);
                
                // Asignar la imagen si fue subida
                if (rutaImagen != null)
                {
                    nuevaComunidad.ImagenUrl = rutaImagen;
                    _comunidadRepository.Modify(nuevaComunidad);
                }

                // Agregar al creador como LIDER de la comunidad
                _miembroComunidadCEN.NewMiembroComunidad(usuarioActual, nuevaComunidad, ApplicationCore.Domain.Enums.RolComunidad.LIDER);

                try { _unitOfWork?.SaveChanges(); } catch { }
                return RedirectToAction("MisComunidades");
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

        // GET: /Comunidad/Miembros/5
        public IActionResult Miembros(long id)
        {
            try
            {
                var en = _comunidadCEN.ReadOID_Comunidad(id);
                if (en == null) return NotFound();
                var vm = ComunidadAssembler.ConvertENToViewModel(en);
                
                // Cargar los miembros de la comunidad (solo activos)
                var miembros = (en.Miembros ?? Enumerable.Empty<MiembroComunidad>())
                    .Where(m => m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA)
                    .Select(m => MiembroComunidadAssembler.ConvertENToViewModel(m))
                    .Where(m => m != null)
                    .OrderBy(m => m?.FechaAlta ?? DateTime.MinValue)
                    .ToList();
                
                ViewBag.Miembros = (object?)miembros;
                
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // GET: /Comunidad/ListaNegra/5
        public IActionResult ListaNegra(long id)
        {
            try
            {
                // Verificar que el usuario actual es líder de la comunidad
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return RedirectToAction("Login", "Usuario");
                }

                var en = _comunidadCEN.ReadOID_Comunidad(id);
                if (en == null) return NotFound();

                // Verificar que el usuario actual es LIDER, COLABORADOR o MODERADOR
                var miembroActual = en.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value 
                    && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                if (miembroActual == null || 
                    (miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.LIDER && 
                     miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.COLABORADOR &&
                     miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.MODERADOR))
                {
                    TempData["ErrorMessage"] = "No tienes permisos para acceder a la lista negra.";
                    return RedirectToAction("Details", new { id });
                }

                var vm = ComunidadAssembler.ConvertENToViewModel(en);
                
                // Cargar los miembros expulsados de la comunidad
                var miembrosExpulsados = (en.Miembros ?? Enumerable.Empty<MiembroComunidad>())
                    .Where(m => m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA)
                    .Select(m => MiembroComunidadAssembler.ConvertENToViewModel(m))
                    .Where(m => m != null)
                    .OrderByDescending(m => m?.FechaBaja ?? DateTime.MinValue)
                    .ToList();
                
                ViewBag.MiembrosExpulsados = (object?)miembrosExpulsados;
                
                return View(vm);
            }
            catch (System.Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        // POST: /Comunidad/PermitirReingreso
        [HttpPost]
        public IActionResult PermitirReingreso(long miembroId)
        {
            try
            {
                // Verificar autenticación
                var uid = HttpContext.Session.GetInt32("UsuarioId");
                if (!uid.HasValue)
                {
                    return Json(new { success = false, message = "No autenticado" });
                }

                // Obtener el miembro expulsado
                var miembro = _miembroComunidadRepository.ReadById(miembroId);
                if (miembro == null)
                {
                    return Json(new { success = false, message = "Miembro no encontrado" });
                }

                // Verificar que está expulsado
                if (miembro.Estado != ApplicationCore.Domain.Enums.EstadoMembresia.EXPULSADA)
                {
                    return Json(new { success = false, message = "Este usuario no está en la lista negra" });
                }

                // Verificar que el usuario actual es líder, colaborador o moderador de la comunidad
                var comunidad = miembro.Comunidad;
                var miembroActual = comunidad.Miembros?.FirstOrDefault(m => m.Usuario.IdUsuario == uid.Value 
                    && m.Estado == ApplicationCore.Domain.Enums.EstadoMembresia.ACTIVA);
                if (miembroActual == null || 
                    (miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.LIDER && 
                     miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.COLABORADOR &&
                     miembroActual.Rol != ApplicationCore.Domain.Enums.RolComunidad.MODERADOR))
                {
                    return Json(new { success = false, message = "No tienes permisos para realizar esta acción" });
                }

                // Cambiar el estado a ABANDONADA para permitir que el usuario pueda volver a unirse
                miembro.Estado = ApplicationCore.Domain.Enums.EstadoMembresia.ABANDONADA;
                _miembroComunidadRepository.Modify(miembro);
                _unitOfWork?.SaveChanges();

                return Json(new { success = true, message = "El usuario ahora puede volver a unirse a la comunidad" });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
