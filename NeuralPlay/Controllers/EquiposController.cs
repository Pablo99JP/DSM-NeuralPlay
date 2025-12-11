using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Assemblers;
using NeuralPlay.Models;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.Enums;
using ApplicationCore.Domain.Repositories;
using Microsoft.AspNetCore.Http;

namespace NeuralPlay.Controllers
{
    public class EquiposController : BasicController
    {
        private readonly MiembroEquipoCEN _miembroEquipoCEN;
        private readonly IMiembroEquipoRepository _miembroEquipoRepository;

        public EquiposController(
            UsuarioCEN usuarioCEN,
            IUsuarioRepository usuarioRepository,
            MiembroEquipoCEN miembroEquipoCEN,
            IMiembroEquipoRepository miembroEquipoRepository)
            : base(usuarioCEN, usuarioRepository)
        {
            _miembroEquipoCEN = miembroEquipoCEN;
            _miembroEquipoRepository = miembroEquipoRepository;
        }

        // GET: /Equipos - Muestra todos los equipos del usuario actual
        public IActionResult Index()
        {
            try
            {
                // Obtener el usuario de la sesión
                var userId = HttpContext.Session.GetInt32("UsuarioId");
                
                if (!userId.HasValue)
                {
                    // Usuario no autenticado - redirigir al login
                    TempData["ErrorMessage"] = "Debes iniciar sesión para ver tus equipos.";
                    return RedirectToAction("Login", "Usuario");
                }

                // Obtener los equipos del usuario usando el método del CEN
                var equipos = _miembroEquipoCEN.ReadFilter_EquiposByUsuarioMembership(userId.Value);
                var vms = EquipoAssembler.ConvertListENToViewModel(equipos).ToList();

                // Marcar si el usuario es admin en cada equipo
                var memberships = _miembroEquipoRepository.ReadAll()
                    .Where(m =>
                        m.Usuario != null && m.Usuario.IdUsuario == userId.Value &&
                        m.Equipo != null &&
                        m.Estado == EstadoMembresia.ACTIVA)
                    .GroupBy(m => m.Equipo!.IdEquipo)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var vm in vms)
                {
                    if (memberships.TryGetValue(vm.IdEquipo, out var membership))
                    {
                        vm.IsLeader = membership.Rol == RolEquipo.ADMIN;
                    }
                }
                
                ViewBag.NoAutenticado = false;
                return View(vms);
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(Enumerable.Empty<EquipoViewModel>());
            }
        }
    }
}
