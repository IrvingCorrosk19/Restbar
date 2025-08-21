using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Models;
using RestBar.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStationService _stationService;
        private readonly IAuthService _authService;

        public HomeController(ILogger<HomeController> logger, IStationService stationService, IAuthService authService)
        {
            _logger = logger;
            _stationService = stationService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "guest";
                var stations = await _stationService.GetAllStationsAsync();
                var visibleCards = GetVisibleCardsForRole(userRole);

                var viewModel = new DashboardViewModel
                {
                    Stations = stations,
                    UserInfo = new UserInfo
                    {
                        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                        Name = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
                        Role = userRole,
                        IsAuthenticated = User.Identity?.IsAuthenticated ?? false
                    },
                    VisibleCards = visibleCards
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HomeController] Error al cargar dashboard");
                return View(new DashboardViewModel());
            }
        }

        /// <summary>
        /// Determina qué cards son visibles para cada rol específico
        /// </summary>
        private CardVisibility GetVisibleCardsForRole(string role)
        {
            var cards = new CardVisibility();
            
            switch (role.ToLower())
            {
                case "admin":
                    // Admin tiene acceso completo
                    cards.Orders = true;
                    cards.Products = true;
                    cards.Tables = true;
                    cards.UserManagement = true;
                    cards.UserAssignments = true;
                    cards.Categories = true;
                    cards.Stations = true;
                    cards.Areas = true;
                    cards.Companies = true;
                    cards.Branches = true;
                    cards.Kitchen = true;
                    cards.Payments = true;
                    cards.Reports = true;

                    cards.Transfers = true;
                    cards.AdvancedSettings = true;
                    cards.AdvancedReports = true;
                    cards.SecurityAdmin = true;
                    break;
                    
                case "manager":
                    // Gerente tiene acceso amplio excepto configuración de sistema
                    cards.Orders = true;
                    cards.Products = true;
                    cards.Tables = true;
                    cards.UserManagement = true;
                    cards.UserAssignments = true;
                    cards.Categories = true;
                    cards.Stations = true;
                    cards.Areas = true;
                    cards.Branches = true;
                    cards.Kitchen = true;
                    cards.Payments = true;
                    cards.Reports = true;

                    cards.Transfers = true;
                    cards.AdvancedSettings = true;
                    cards.AdvancedReports = true;
                    cards.Companies = false; // Solo admin
                    cards.SecurityAdmin = false; // Solo admin
                    break;
                    
                case "supervisor":
                    // Supervisor tiene acceso operacional
                    cards.Orders = true;
                    cards.Tables = true;
                    cards.Kitchen = true;
                    cards.Payments = true;
                    cards.Areas = true;
                    cards.UserAssignments = true;
                    cards.Products = false; // Solo admin, manager, inventory
                    cards.UserManagement = false; // Solo admin, manager, support
                    cards.Categories = false; // Solo admin, manager
                    cards.Stations = false; // Solo admin, manager
                    cards.Companies = false; // Solo admin
                    cards.Branches = false; // Solo admin, manager
                    cards.Reports = false; // Solo admin, manager, accountant

                    cards.SecurityAdmin = false; // Solo admin
                    break;
                    
                case "waiter":
                    // Mesero solo acceso a órdenes y mesas
                    cards.Orders = true;
                    cards.Tables = true;
                    cards.Kitchen = false;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "cashier":
                    // Cajero acceso a órdenes y pagos
                    cards.Orders = true;
                    cards.Payments = true;
                    cards.Tables = true;
                    cards.Kitchen = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "chef":
                    // Cocinero acceso a cocina y órdenes
                    cards.Kitchen = true;
                    cards.Orders = true;
                    cards.Tables = false;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "bartender":
                    // Bartender acceso a cocina y órdenes
                    cards.Kitchen = true;
                    cards.Orders = true;
                    cards.Tables = false;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "inventory":
                    // Inventarista acceso a productos e inventario
                    cards.Products = true;

                    cards.Transfers = true;
                    cards.Categories = true;
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Kitchen = false;
                    cards.Payments = false;
                    cards.UserManagement = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "accountant":
                    // Contador acceso a pagos y reportes
                    cards.Payments = true;
                    cards.Reports = true;
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Kitchen = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "support":
                    // Soporte acceso a usuarios
                    cards.UserManagement = true;
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Kitchen = false;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.Inventory = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                default:
                    // Sin rol, sin acceso
                    break;
            }
            
            return cards;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    
    // Modelos de vista para el dashboard
    public class DashboardViewModel
    {
        public IEnumerable<Station> Stations { get; set; } = new List<Station>();
        public UserInfo UserInfo { get; set; } = new UserInfo();
        public CardVisibility VisibleCards { get; set; } = new CardVisibility();
    }
    
    public class UserInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsAuthenticated { get; set; } = false;
    }
    
    public class CardVisibility
    {
        public bool Orders { get; set; } = false;
        public bool Products { get; set; } = false;
        public bool Tables { get; set; } = false;
        public bool UserManagement { get; set; } = false;
        public bool UserAssignments { get; set; } = false;
        public bool Categories { get; set; } = false;
        public bool Stations { get; set; } = false;
        public bool Areas { get; set; } = false;
        public bool Companies { get; set; } = false;
        public bool Branches { get; set; } = false;
        public bool Kitchen { get; set; } = false;
        public bool Payments { get; set; } = false;
        public bool Reports { get; set; } = false;

        public bool Transfers { get; set; } = false;
        public bool AdvancedSettings { get; set; } = false;
        public bool AdvancedReports { get; set; } = false;
        public bool SecurityAdmin { get; set; } = false;
    }
}
