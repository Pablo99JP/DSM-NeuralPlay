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

        public HomeController(ILogger<HomeController> logger, ApplicationCore.Domain.CEN.ComunidadCEN comunidadCEN)
        {
            _logger = logger;
            _comunidadCEN = comunidadCEN;
        }

        public IActionResult Index()
        {
            try
            {
                // Obtener todas las comunidades
                var comunidades = _comunidadCEN.ReadAll_Comunidad();
                var listaComunidadesVM = NeuralPlay.Assemblers.ComunidadAssembler.ConvertListENToViewModel(comunidades).ToList();
                return View(listaComunidadesVM);
            }
            catch (System.Exception ex)
            {
                _logger?.LogWarning(ex, "Fallo al cargar comunidades para la vista Home: {Message}", ex.Message);
                return View(System.Linq.Enumerable.Empty<NeuralPlay.Models.ComunidadViewModel>());
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