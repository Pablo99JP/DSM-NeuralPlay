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
        private readonly ApplicationCore.Domain.CEN.JuegoCEN _juegoCEN;

        public HomeController(ILogger<HomeController> logger, ApplicationCore.Domain.CEN.JuegoCEN juegoCEN)
        {
            _logger = logger;
            _juegoCEN = juegoCEN;
        }

        public IActionResult Index()
        {
            try
            {
                // Obtener lista de juegos mediante el CEN (usa el repositorio NHibernate inyectado por DI)
                var listaJuegos = _juegoCEN.ReadAll_Juego();
                // Log the concrete runtime type to help debug model type issues
                var runtimeType = listaJuegos?.GetType().FullName ?? "(null)";
                _logger?.LogDebug("Home.Index: juego collection runtime type = {Type}", runtimeType);
                System.Console.WriteLine($"DEBUG: Home.Index juego collection runtime type = {runtimeType}");
                return View(listaJuegos);
            }
            catch (System.Exception ex)
            {
                // Si hay cualquier problema, logueamos y devolvemos una lista vacía del tipo esperado
                _logger?.LogWarning(ex, "Fallo al cargar juegos para la vista Home: {Message}", ex.Message);
                return View(System.Linq.Enumerable.Empty<ApplicationCore.Domain.EN.Juego>());
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