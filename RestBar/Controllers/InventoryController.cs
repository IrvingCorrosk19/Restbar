using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    [Authorize(Policy = "InventoryAccess")]
    public class InventoryController : Controller
    {
        private readonly RestBarContext _context;
        private readonly IProductService _productService;
        private readonly IAreaService _areaService;

        public InventoryController(
            RestBarContext context,
            IProductService productService,
            IAreaService areaService)
        {
            _context = context;
            _productService = productService;
            _areaService = areaService;
        }

        // GET: Inventory/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Inventory/LowStockProducts
        /// <summary>
        /// Obtiene productos con stock bajo (Stock <= MinStock)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLowStockProducts()
        {
            try
            {
                Console.WriteLine("🔍 [InventoryController] GetLowStockProducts() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // Obtener productos con inventario activo
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.StockAssignments.Where(sa => sa.IsActive && sa.BranchId == currentUser.BranchId))
                    .Where(p => p.IsActive && 
                               p.TrackInventory && 
                               (p.BranchId == currentUser.BranchId || p.BranchId == null))
                    .ToListAsync();

                var lowStockProducts = new List<object>();

                foreach (var product in products)
                {
                    // Verificar stock global
                    if (product.Stock.HasValue && product.MinStock.HasValue)
                    {
                        if (product.Stock <= product.MinStock)
                        {
                            var availableStock = await _productService.GetAvailableStockAsync(product.Id, currentUser.BranchId);
                            lowStockProducts.Add(new
                            {
                                productId = product.Id,
                                productName = product.Name,
                                stock = product.Stock.Value,
                                minStock = product.MinStock.Value,
                                availableStock = availableStock,
                                categoryName = product.Category?.Name ?? "Sin categoría",
                                // ✅ ELIMINADO: stationName - Ya no se usa Station en Product
                                type = "global"
                            });
                        }
                    }

                    // Verificar stock por estación
                    foreach (var assignment in product.StockAssignments)
                    {
                        if (assignment.MinStock.HasValue && assignment.Stock <= assignment.MinStock.Value)
                        {
                            var station = await _context.Stations.FindAsync(assignment.StationId);
                            lowStockProducts.Add(new
                            {
                                productId = product.Id,
                                productName = product.Name,
                                stock = assignment.Stock,
                                minStock = assignment.MinStock.Value,
                                availableStock = assignment.Stock,
                                categoryName = product.Category?.Name ?? "Sin categoría",
                                stationName = station?.Name ?? "Sin estación",
                                type = "station",
                                stationId = assignment.StationId
                            });
                        }
                    }
                }

                Console.WriteLine($"✅ [InventoryController] GetLowStockProducts() - Total productos con stock bajo: {lowStockProducts.Count}");
                return Json(new { success = true, data = lowStockProducts });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] GetLowStockProducts() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] GetLowStockProducts() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/GetInventoryData
        /// <summary>
        /// Obtiene todos los datos de inventario para la vista principal
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetInventoryData()
        {
            try
            {
                Console.WriteLine("🔍 [InventoryController] GetInventoryData() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                // Obtener productos con inventario activo
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Branch)
                    .Where(p => p.IsActive && 
                               p.TrackInventory && 
                               (p.BranchId == currentUser.BranchId || p.BranchId == null))
                    .ToListAsync();

                var inventoryData = products.Select(p => new
                {
                    productId = p.Id,
                    productName = p.Name,
                    productDescription = p.Description,
                    categoryId = p.CategoryId,
                    categoryName = p.Category?.Name ?? "Sin categoría",
                    branchId = p.BranchId ?? currentUser.BranchId,
                    branchName = p.Branch?.Name ?? currentUser.Branch?.Name ?? "Sin sucursal",
                    quantity = p.Stock ?? 0,
                    minStock = p.MinStock ?? 0,
                    lastUpdated = p.UpdatedAt
                }).ToList();

                Console.WriteLine($"✅ [InventoryController] GetInventoryData() - Total productos: {inventoryData.Count}");
                return Json(new { success = true, inventory = inventoryData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] GetInventoryData() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] GetInventoryData() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/GetProducts
        /// <summary>
        /// Obtiene todos los productos para modales
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                Console.WriteLine("🔍 [InventoryController] GetProducts() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && 
                               (p.BranchId == currentUser.BranchId || p.BranchId == null))
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        description = p.Description,
                        categoryId = p.CategoryId,
                        categoryName = p.Category != null ? p.Category.Name : "Sin categoría"
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ [InventoryController] GetProducts() - Total productos: {products.Count}");
                return Json(new { success = true, products = products });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] GetProducts() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] GetProducts() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/GetCategories
        /// <summary>
        /// Obtiene todas las categorías para modales
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                Console.WriteLine("🔍 [InventoryController] GetCategories() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var categories = await _context.Categories
                    .Where(c => c.IsActive && 
                               (c.BranchId == currentUser.BranchId || c.BranchId == null))
                    .Select(c => new
                    {
                        id = c.Id,
                        name = c.Name,
                        description = c.Description
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ [InventoryController] GetCategories() - Total categorías: {categories.Count}");
                return Json(new { success = true, categories = categories });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] GetCategories() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] GetCategories() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/GetBranches
        /// <summary>
        /// Obtiene todas las sucursales para modales
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                Console.WriteLine("🔍 [InventoryController] GetBranches() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var branches = await _context.Branches
                    .Where(b => b.IsActive && b.CompanyId == currentUser.Branch.CompanyId)
                    .Select(b => new
                    {
                        id = b.Id,
                        name = b.Name,
                        address = b.Address,
                        phone = b.Phone
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ [InventoryController] GetBranches() - Total sucursales: {branches.Count}");
                return Json(new { success = true, branches = branches });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] GetBranches() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] GetBranches() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/ConsumptionReport
        /// <summary>
        /// Genera reporte de consumo por producto y estación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConsumptionReport(DateTime? startDate, DateTime? endDate, Guid? productId, Guid? stationId)
        {
            try
            {
                Console.WriteLine($"🔍 [InventoryController] ConsumptionReport() - Iniciando...");
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Json(new { success = false, message = "Usuario no autenticado" });
                }

                var currentUser = await _areaService.GetCurrentUserWithAssignmentsAsync(Guid.Parse(userIdClaim.Value));
                if (currentUser?.Branch == null)
                {
                    return Json(new { success = false, message = "Usuario o sucursal no encontrado" });
                }

                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today;

                Console.WriteLine($"📋 [InventoryController] ConsumptionReport() - Filtros: StartDate={start}, EndDate={end}, ProductId={productId}, StationId={stationId}, BranchId={currentUser.BranchId}");

                // Obtener OrderItems con sus detalles
                var orderItemsQuery = _context.OrderItems
                    .Include(oi => oi.Product)
                    .Include(oi => oi.PreparedByStation)
                    .Include(oi => oi.Order)
                    .Where(oi => oi.Order.BranchId == currentUser.BranchId &&
                                oi.Order.CreatedAt >= start &&
                                oi.Order.CreatedAt <= end &&
                                oi.Product != null);

                if (productId.HasValue)
                {
                    orderItemsQuery = orderItemsQuery.Where(oi => oi.ProductId == productId.Value);
                }

                if (stationId.HasValue)
                {
                    orderItemsQuery = orderItemsQuery.Where(oi => oi.PreparedByStationId == stationId.Value);
                }

                var orderItems = await orderItemsQuery.ToListAsync();

                // Agrupar por producto y estación
                var consumptionData = orderItems
                    .GroupBy(oi => new
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        StationId = oi.PreparedByStationId,
                        StationName = oi.PreparedByStation != null ? oi.PreparedByStation.Name : "Sin estación"
                    })
                    .Select(g => new
                    {
                        productId = g.Key.ProductId,
                        productName = g.Key.ProductName,
                        stationId = g.Key.StationId,
                        stationName = g.Key.StationName,
                        totalQuantity = g.Sum(oi => oi.Quantity),
                        totalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                        averageQuantity = g.Average(oi => oi.Quantity),
                        minQuantity = g.Min(oi => oi.Quantity),
                        maxQuantity = g.Max(oi => oi.Quantity)
                    })
                    .OrderByDescending(x => x.totalQuantity)
                    .ToList();

                Console.WriteLine($"✅ [InventoryController] ConsumptionReport() - Total registros: {consumptionData.Count}");
                return Json(new { success = true, data = consumptionData, filters = new { startDate = start, endDate = end, productId, stationId } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [InventoryController] ConsumptionReport() - Error: {ex.Message}");
                Console.WriteLine($"🔍 [InventoryController] ConsumptionReport() - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

