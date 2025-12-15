using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Necesario para manejo de archivos (fotos)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting; // Necesario para IWebHostEnvironment
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Repositories;

namespace NeuralPlay.Controllers
{
    public class UsuarioController : BasicController
    {
        // Dependencias principales
        private readonly ApplicationCore.Domain.Repositories.IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        
        // CENs adicionales para gestión de relaciones
        private readonly PerfilCEN _perfilCEN;
        private readonly MiembroComunidadCEN _miembroComunidadCEN;
        private readonly MiembroEquipoCEN _miembroEquipoCEN;

        // Repositorios genéricos para limpieza de entidades "satélite" (FKs)
        private readonly IRepository<SolicitudIngreso> _solicitudRepository;
        private readonly IRepository<MensajeChat> _mensajeRepository;
        private readonly IRepository<Comentario> _comentarioRepository;
        private readonly IRepository<Publicacion> _publicacionRepository;
        private readonly IRepository<Reaccion> _reaccionRepository;
        private readonly IRepository<VotoTorneo> _votoTorneoRepository;
        private readonly IRepository<Notificacion> _notificacionRepository;
        private readonly IRepository<Invitacion> _invitacionRepository;
        private readonly IRepository<PropuestaTorneo> _propuestaTorneoRepository;

        // Constructor con inyección de todas las dependencias
        public UsuarioController(
            UsuarioCEN usuarioCEN, 
            IUsuarioRepository usuarioRepository, 
            ApplicationCore.Domain.Repositories.IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            PerfilCEN perfilCEN,
            MiembroComunidadCEN miembroComunidadCEN,
            MiembroEquipoCEN miembroEquipoCEN,
            IRepository<SolicitudIngreso> solicitudRepository,
            IRepository<MensajeChat> mensajeRepository,
            IRepository<Comentario> comentarioRepository,
            IRepository<Publicacion> publicacionRepository,
            IRepository<Reaccion> reaccionRepository,
            IRepository<VotoTorneo> votoTorneoRepository,
            IRepository<Notificacion> notificacionRepository,
            IRepository<Invitacion> invitacionRepository,
            IRepository<PropuestaTorneo> propuestaTorneoRepository)
            : base(usuarioCEN, usuarioRepository)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _perfilCEN = perfilCEN;
            _miembroComunidadCEN = miembroComunidadCEN;
            _miembroEquipoCEN = miembroEquipoCEN;
            
            _solicitudRepository = solicitudRepository;
            _mensajeRepository = mensajeRepository;
            _comentarioRepository = comentarioRepository;
            _publicacionRepository = publicacionRepository;
            _reaccionRepository = reaccionRepository;
            _votoTorneoRepository = votoTorneoRepository;
            _notificacionRepository = notificacionRepository;
            _invitacionRepository = invitacionRepository;
            _propuestaTorneoRepository = propuestaTorneoRepository;
        }

        // GET: /Usuario
        public IActionResult Index()
        {
            var usuarios = _usuarioCEN.ReadAll_Usuario();
            var vmList = UsuarioAssembler.ConvertListENToModel(usuarios);
            return View(vmList);
        }

        // GET: /Usuario/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UsuarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Validar que no exista otro usuario con el mismo email
                if (_usuarioCEN.ExisteEmail(model.Email ?? string.Empty))
                {
                    ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo electrónico.");
                    return View(model);
                }

                // Validar que no exista otro usuario con el mismo nick
                if (_usuarioCEN.ExisteNick(model.Nombre ?? string.Empty))
                {
                    ModelState.AddModelError(nameof(model.Nombre), "Ya existe un usuario con este nombre.");
                    return View(model);
                }

                // Crear usuario con contraseña hasheada
                var usuarioCreado = _usuarioCEN.NewUsuario(model.Nombre ?? string.Empty, model.Email ?? string.Empty, model.Password ?? string.Empty);
                
                // Persistir cambios via UnitOfWork
                try { _unitOfWork?.SaveChanges(); } catch { }
                
                // Validar creación
                if (usuarioCreado == null || usuarioCreado.IdUsuario == 0)
                {
                    ModelState.AddModelError(string.Empty, "Error al crear el usuario. Intenta de nuevo.");
                    return View(model);
                }

                // Login automático
                HttpContext.Session.SetInt32("UsuarioId", (int)usuarioCreado.IdUsuario);
                HttpContext.Session.SetString("UsuarioNombre", usuarioCreado.Nick ?? string.Empty);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
                return View(model);
            }
        }

        // GET: /Usuario/Edit/5
        public IActionResult Edit(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }

        // POST: /Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UsuarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var en = _usuarioCEN.ReadOID_Usuario(model.Id);
            if (en == null) return NotFound();

            en.Nick = model.Nombre;
            en.CorreoElectronico = model.Email;
            en.ContrasenaHash = model.Password; 

            _usuarioCEN.ModifyUsuario(en);
            try { _unitOfWork?.SaveChanges(); } catch { }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Usuario/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = _usuarioCEN.Login(model.Email ?? string.Empty, model.Password ?? string.Empty);
            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas");
                return View(model);
            }

            HttpContext.Session.SetInt32("UsuarioId", (int)usuario.IdUsuario);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nick ?? string.Empty);

            return RedirectToAction("Index", "Home");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: /Usuario/Delete/5
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult DeleteConfirm(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }

        // POST: /Usuario/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id)
        {
            try 
            {
                var usuario = _usuarioCEN.ReadOID_Usuario(id);
                if (usuario != null)
                {
                    // --- FASE 1: LIMPIEZA DE ENTIDADES SATÉLITE (FKs bloqueantes) ---
                    
                    // 1. Eliminar Solicitudes de Ingreso (Como solicitante)
                    var solicitudes = _solicitudRepository.ReadAll()
                        .Where(s => s.Solicitante != null && s.Solicitante.IdUsuario == id).ToList();
                    foreach(var sol in solicitudes) _solicitudRepository.Destroy(sol.IdSolicitud);

                    // 2. Eliminar Mensajes de Chat
                    var mensajes = _mensajeRepository.ReadAll()
                        .Where(m => m.Autor != null && m.Autor.IdUsuario == id).ToList();
                    foreach(var msg in mensajes) _mensajeRepository.Destroy(msg.IdMensajeChat);

                    // 3. Eliminar Comentarios
                    var comentarios = _comentarioRepository.ReadAll()
                        .Where(c => c.Autor != null && c.Autor.IdUsuario == id).ToList();
                    foreach(var com in comentarios) _comentarioRepository.Destroy(com.IdComentario);

                    // 4. Eliminar Publicaciones
                    var publicaciones = _publicacionRepository.ReadAll()
                        .Where(p => p.Autor != null && p.Autor.IdUsuario == id).ToList();
                    foreach(var pub in publicaciones) _publicacionRepository.Destroy(pub.IdPublicacion);
                    
                    // 5. Eliminar Reacciones
                    var reacciones = _reaccionRepository.ReadAll()
                        .Where(r => r.Autor != null && r.Autor.IdUsuario == id).ToList();
                    foreach(var reac in reacciones) _reaccionRepository.Destroy(reac.IdReaccion);

                    // 6. Eliminar Votos en Torneos
                    var votos = _votoTorneoRepository.ReadAll()
                        .Where(v => v.Votante != null && v.Votante.IdUsuario == id).ToList();
                    foreach(var voto in votos) _votoTorneoRepository.Destroy(voto.IdVoto);

                    // 7. Eliminar Notificaciones (como destinatario)
                    var notificaciones = _notificacionRepository.ReadAll()
                        .Where(n => n.Destinatario != null && n.Destinatario.IdUsuario == id).ToList();
                    foreach(var notif in notificaciones) _notificacionRepository.Destroy(notif.IdNotificacion);

                    // 8. Eliminar Invitaciones (como emisor o destinatario)
                    var invitaciones = _invitacionRepository.ReadAll()
                        .Where(i => (i.Emisor != null && i.Emisor.IdUsuario == id) || 
                                    (i.Destinatario != null && i.Destinatario.IdUsuario == id)).ToList();
                    foreach(var inv in invitaciones) _invitacionRepository.Destroy(inv.IdInvitacion);

                    // 9. Eliminar Propuestas de Torneo
                    var propuestas = _propuestaTorneoRepository.ReadAll()
                        .Where(p => p.PropuestoPor != null && p.PropuestoPor.IdUsuario == id).ToList();
                    foreach(var prop in propuestas) _propuestaTorneoRepository.Destroy(prop.IdPropuesta);


                    // --- FASE 2: LIMPIEZA DE RELACIONES DIRECTAS (Mapeadas en Usuario) ---

                    if (usuario.MiembrosComunidad != null)
                    {
                        foreach(var miembro in usuario.MiembrosComunidad.ToList())
                        {
                            _miembroComunidadCEN.DestroyMiembroComunidad(miembro.IdMiembroComunidad);
                        }
                    }

                    if (usuario.MiembrosEquipo != null)
                    {
                        foreach(var miembro in usuario.MiembrosEquipo.ToList())
                        {
                            _miembroEquipoCEN.DestroyMiembroEquipo(miembro.IdMiembroEquipo);
                        }
                    }

                    // Borrado de Perfil y Foto física
                    if (usuario.Perfil != null)
                    {
                        // Solo borrar la foto si existe, no es Default.png y no está vacía
                        if (!string.IsNullOrEmpty(usuario.Perfil.FotoPerfilUrl) && 
                            !usuario.Perfil.FotoPerfilUrl.Contains("Default.png", StringComparison.OrdinalIgnoreCase))
                        {
                            string webRootPath = _webHostEnvironment.WebRootPath;
                            // Eliminar barra inicial si existe
                            string path = usuario.Perfil.FotoPerfilUrl.TrimStart('/', '\\');
                            string fullPath = Path.Combine(webRootPath, path);
                            
                            // Solo intentar borrar si el archivo existe y no es Default.png
                            if (System.IO.File.Exists(fullPath))
                            {
                                try
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                                catch
                                {
                                    // Si falla el borrado de la foto, continuar con el resto del proceso
                                    // (no queremos que falle todo por un error de archivo)
                                }
                            }
                        }
                        _perfilCEN.DestroyPerfil(usuario.Perfil.IdPerfil);
                    }

                    // --- FASE 3: BORRADO FINAL DEL USUARIO ---
                    
                    _usuarioCEN.DestroyUsuario(id);
                    
                    // Confirmar transacción
                    _unitOfWork?.SaveChanges();
                    
                    // Si el usuario borrado es el logueado actualmente, cerrar sesión
                    var currentUserId = HttpContext.Session.GetInt32("UsuarioId");
                    if (currentUserId.HasValue && currentUserId.Value == id)
                    {
                        return Logout();
                    }
                }
            }
            catch (Exception ex)
            {
                // En caso de error, podrías registrar el log o mostrar un mensaje
                // Por ahora redirigimos al Index
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Usuario/Details/5
        public IActionResult Details(long id)
        {
            var en = _usuarioCEN.ReadOID_Usuario(id);
            if (en == null) return NotFound();

            var vm = UsuarioAssembler.ConvertENToViewModel(en);
            return View(vm);
        }
    }
}