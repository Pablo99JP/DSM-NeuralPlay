using ApplicationCore.Domain.Repositories;
using Infrastructure.NHibernate;
using Microsoft.AspNetCore.Mvc;
using NeuralPlay.Models;
using System.Diagnostics;
using NHibernate;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.EN; // Aseg rate de tener esta referencia si usas NHibernate

namespace NeuralPlay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationCore.Domain.CEN.ComunidadCEN _comunidadCEN;
        private readonly ApplicationCore.Domain.CEN.EquipoCEN _equipoCEN;

        public HomeController(ILogger<HomeController> logger, 
            ApplicationCore.Domain.CEN.ComunidadCEN comunidadCEN,
            ApplicationCore.Domain.CEN.EquipoCEN equipoCEN)
        {
            _logger = logger;
            _comunidadCEN = comunidadCEN;
            _equipoCEN = equipoCEN;
        }

        public IActionResult Index()
        {
            try
            {
                // Obtener todas las comunidades
                var comunidades = _comunidadCEN.ReadAll_Comunidad();
                var listaComunidadesVM = NeuralPlay.Assemblers.ComunidadAssembler.ConvertListENToViewModel(comunidades).ToList();

                // Obtener todos los equipos
                var equipos = _equipoCEN.ReadAll_Equipo();
                var listaEquiposVM = NeuralPlay.Assemblers.EquipoAssembler.ConvertListENToViewModel(equipos).ToList();

                // Crear modelo consolidado
                var modelo = new ExplorarViewModel
                {
                    Comunidades = listaComunidadesVM,
                    Equipos = listaEquiposVM
                };

                return View(modelo);
            }
            catch (System.Exception ex)
            {
                _logger?.LogWarning(ex, "Fallo al cargar datos para la vista Home: {Message}", ex.Message);
                return View(new ExplorarViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}