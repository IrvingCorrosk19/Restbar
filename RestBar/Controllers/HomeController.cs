using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RestBar.Models;
using RestBar.Interfaces;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStationService _stationService;

        public HomeController(ILogger<HomeController> logger, IStationService stationService)
        {
            _logger = logger;
            _stationService = stationService;
        }

        public async Task<IActionResult> Index()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return View(stations);
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
