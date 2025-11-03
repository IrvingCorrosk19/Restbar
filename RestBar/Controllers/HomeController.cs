using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Models;
using RestBar.Interfaces;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace RestBar.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStationService _stationService;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public HomeController(ILogger<HomeController> logger, IStationService stationService, IAuthService authService, IUserService userService)
        {
            _logger = logger;
            _stationService = stationService;
            _authService = authService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("🔍 [HomeController] Index() - Iniciando...");
                
                // Obtener información del usuario actual primero
                var userRole = User.FindFirst("UserRole")?.Value ?? "";
                var userId = User.FindFirst("UserId")?.Value ?? "";
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
                
                Console.WriteLine($"✅ [HomeController] Index() - Usuario: {userName}, Rol: {userRole}");
                
                // Obtener estaciones con manejo de errores
                List<Station> stations = new List<Station>();
                try
                {
                    var stationsResult = await _stationService.GetAllStationsAsync();
                    stations = stationsResult?.ToList() ?? new List<Station>();
                    Console.WriteLine($"📊 [HomeController] Index() - Estaciones obtenidas: {stations.Count}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [HomeController] Index() - Error al obtener estaciones: {ex.Message}");
                    // Continuar con lista vacía si falla
                    stations = new List<Station>();
                }
                
                // ✅ NUEVO: Obtener nombres de estaciones únicos para crear cards dinámicas
                List<string> stationNames = new List<string>();
                try
                {
                    var stationNamesResult = await _stationService.GetDistinctStationTypesAsync();
                    stationNames = stationNamesResult?.ToList() ?? new List<string>();
                    Console.WriteLine($"📊 [HomeController] Index() - Tipos de estaciones obtenidos: {stationNames.Count}");
                    if (stationNames != null)
                    {
                        foreach (var name in stationNames)
                        {
                            Console.WriteLine($"📋 [HomeController] Index() - Tipo de Estación: {name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [HomeController] Index() - Error al obtener tipos de estaciones: {ex.Message}");
                    // Continuar con lista vacía si falla
                    stationNames = new List<string>();
                }
                
                // Crear modelo de vista con información del usuario y permisos
                var viewModel = new DashboardViewModel
                {
                    Stations = stations,
                    StationTypes = stationNames,
                    UserInfo = new UserInfo
                    {
                        Id = userId,
                        Name = userName,
                        Role = userRole,
                        IsAuthenticated = User.Identity?.IsAuthenticated ?? false
                    },
                    VisibleCards = GetVisibleCardsForRole(userRole)
                };
                
                Console.WriteLine($"✅ [HomeController] Index() - Dashboard cargado exitosamente");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [HomeController] Index() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [HomeController] Index() - StackTrace: {ex.StackTrace}");
                _logger.LogError(ex, "Error en HomeController.Index()");
                
                // Retornar modelo vacío en caso de error para evitar NullReferenceException
                var errorViewModel = new DashboardViewModel
                {
                    Stations = new List<Station>(),
                    StationTypes = new List<string>(),
                    UserInfo = new UserInfo
                    {
                        IsAuthenticated = User.Identity?.IsAuthenticated ?? false
                    },
                    VisibleCards = new CardVisibility()
                };
                
                return View(errorViewModel);
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
                    // Admin tiene acceso a todo
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
                    cards.Payments = true;
                    cards.Reports = true;
                    cards.AdvancedSettings = true;
                    cards.AdvancedReports = true;
                    cards.SecurityAdmin = true;
                    // ✅ NUEVO: Acceso a órdenes por estación
                    cards.KitchenOrders = true;
                    cards.BarOrders = true;
                    // ✅ NUEVO: Acceso a Inventario
                    cards.Inventory = true;
                    cards.StockAssignments = true;
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
                    cards.Payments = true;
                    cards.Reports = true;
                    cards.AdvancedSettings = true;
                    cards.AdvancedReports = true;
                    cards.Companies = false; // Solo admin
                    cards.SecurityAdmin = false; // Solo admin
                    // ✅ NUEVO: Acceso a órdenes por estación
                    cards.KitchenOrders = true;
                    cards.BarOrders = true;
                    // ✅ NUEVO: Acceso a Inventario
                    cards.Inventory = true;
                    cards.StockAssignments = true;
                    break;
                    
                case "supervisor":
                    // Supervisor tiene acceso operacional
                    cards.Orders = true;
                    cards.Tables = true;
                    cards.Payments = true;
                    cards.Areas = true;
                    cards.UserAssignments = true;
                    cards.Products = false; // Solo admin, manager
                    cards.UserManagement = false; // Solo admin, manager, support
                    cards.Categories = false; // Solo admin, manager
                    cards.Stations = false; // Solo admin, manager
                    cards.Companies = false; // Solo admin
                    cards.Branches = false; // Solo admin, manager
                    cards.Reports = false; // Solo admin, manager, accountant
                    cards.SecurityAdmin = false; // Solo admin
                    // ✅ NUEVO: Acceso a órdenes por estación
                    cards.KitchenOrders = true;
                    cards.BarOrders = true;
                    // ✅ NUEVO: Acceso a Inventario (para ver reportes y alertas)
                    cards.Inventory = true;
                    break;
                    
                case "waiter":
                    // Mesero solo acceso a órdenes y mesas
                    cards.Orders = true;
                    cards.Tables = true;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.SecurityAdmin = false;
                    // ✅ NUEVO: Acceso a órdenes por estación
                    cards.KitchenOrders = true;
                    cards.BarOrders = true;
                    break;
                    
                case "cashier":
                    // Cajero acceso a órdenes y pagos
                    cards.Orders = true;
                    cards.Payments = true;
                    cards.Tables = true;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.SecurityAdmin = false;
                    // ✅ NUEVO: Acceso a órdenes por estación
                    cards.KitchenOrders = true;
                    cards.BarOrders = true;
                    break;
                    
                case "chef":
                    // Cocinero acceso a órdenes
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
                    cards.SecurityAdmin = false;
                    // ✅ NUEVO: Acceso específico a órdenes de cocina
                    cards.KitchenOrders = true;
                    cards.BarOrders = false;
                    break;
                    
                case "bartender":
                    // Bartender acceso a órdenes
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
                    cards.SecurityAdmin = false;
                    // ✅ NUEVO: Acceso específico a órdenes de bar
                    cards.KitchenOrders = false;
                    cards.BarOrders = true;
                    break;
                    
                case "accountant":
                    // Contador acceso a pagos y reportes
                    cards.Payments = true;
                    cards.Reports = true;
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Products = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.SecurityAdmin = false;
                    // ✅ NUEVO: Acceso a Inventario (para reportes financieros)
                    cards.Inventory = true;
                    break;
                    
                case "support":
                    // Soporte acceso a usuarios
                    cards.UserManagement = true;
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Payments = false;
                    cards.Products = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.Reports = false;
                    cards.SecurityAdmin = false;
                    break;
                    
                case "inventarista":
                    // Inventarista acceso completo a inventario
                    cards.Inventory = true;
                    cards.StockAssignments = true;
                    cards.Products = true; // Para ver productos
                    cards.Reports = true; // Para reportes de inventario
                    cards.AdvancedReports = true; // Para reportes avanzados de inventario
                    cards.Orders = false;
                    cards.Tables = false;
                    cards.Payments = false;
                    cards.UserManagement = false;
                    cards.Categories = false;
                    cards.Stations = false;
                    cards.Areas = false;
                    cards.Companies = false;
                    cards.Branches = false;
                    cards.SecurityAdmin = false;
                    cards.KitchenOrders = false;
                    cards.BarOrders = false;
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
        public List<string> StationTypes { get; set; } = new List<string>(); // ✅ NUEVO: Tipos de estaciones únicos
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

        public bool Payments { get; set; } = false;
        public bool Reports { get; set; } = false;

        public bool AdvancedSettings { get; set; } = false;
        public bool AdvancedReports { get; set; } = false;
        public bool SecurityAdmin { get; set; } = false;
        
        // ✅ NUEVO: Cards para órdenes por estación
        public bool KitchenOrders { get; set; } = false;
        public bool BarOrders { get; set; } = false;
        
        // ✅ NUEVO: Cards para inventario
        public bool Inventory { get; set; } = false;
        public bool StockAssignments { get; set; } = false;
    }
}
