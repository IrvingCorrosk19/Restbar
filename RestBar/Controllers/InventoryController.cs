using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System.Threading.Tasks;

namespace RestBar.Controllers
{
    [Authorize(Policy = "InventoryAccess")]
    public class InventoryController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly IBranchService _branchService;
        private readonly ICategoryService _categoryService;

        public InventoryController(
            IInventoryService inventoryService,
            IProductService productService,
            IBranchService branchService,
            ICategoryService categoryService)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _branchService = branchService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener todos los productos con su stock
                var products = await _productService.GetAllAsync();
                
                // Obtener items de bajo stock
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                
                // Obtener todas las sucursales
                var branches = await _branchService.GetAllAsync();

                var viewModel = new InventoryViewModel
                {
                    Products = products,
                    LowStockItems = lowStockItems,
                    Branches = branches
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error en Index: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(Guid productId, Guid branchId, decimal newQuantity)
        {
            try
            {
                var inventory = await _inventoryService.AdjustStockAsync(productId, branchId, newQuantity);
                
                return Json(new { 
                    success = true, 
                    message = "Stock actualizado exitosamente",
                    newQuantity = inventory.Quantity
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al actualizar stock: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al actualizar stock: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductStock(Guid productId, Guid branchId)
        {
            try
            {
                var inventory = await _inventoryService.GetByBranchAndProductAsync(branchId, productId);
                
                return Json(new { 
                    success = true, 
                    quantity = inventory?.Stock ?? 0,
                    lastUpdated = inventory?.LastUpdated
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener stock: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener stock: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLowStockReport()
        {
            try
            {
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                
                return Json(new { 
                    success = true, 
                    items = lowStockItems.Select(item => new {
                        productName = item.Product?.Name,
                        branchName = item.Branch?.Name,
                        currentStock = item.Stock ?? 0,
                        lastUpdated = item.LastUpdated
                    })
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener reporte de bajo stock: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener reporte: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInventoryData()
        {
            try
            {
                Console.WriteLine($"[InventoryController] GetInventoryData iniciado");
                
                var inventoryItems = await _inventoryService.GetAllAsync();
                Console.WriteLine($"[InventoryController] Inventario obtenido: {inventoryItems.Count()} items");
                
                var inventoryData = inventoryItems.Select(item => new {
                    productId = item.ProductId,
                    productName = item.Product?.Name,
                    productDescription = item.Product?.Description,
                    categoryId = item.Product?.CategoryId,
                    categoryName = item.Product?.Category?.Name,
                    branchId = item.BranchId,
                    branchName = item.Branch?.Name,
                    quantity = item.Stock ?? 0,
                    minStock = item.MinStock ?? 0,
                    maxStock = item.MaxStock ?? 0,
                    lastUpdated = item.LastUpdated
                }).ToList();

                Console.WriteLine($"[InventoryController] Datos procesados: {inventoryData.Count} items");
                
                // Log detallado de cada item
                foreach (var item in inventoryData)
                {
                    Console.WriteLine($"[InventoryController] Item: {item.productName} - Stock: {item.quantity} - Categoría: {item.categoryName} - Sucursal: {item.branchName}");
                }

                return Json(new { 
                    success = true, 
                    inventory = inventoryData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener datos de inventario: {ex.Message}");
                Console.WriteLine($"[InventoryController] Stack trace: {ex.StackTrace}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener datos de inventario: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            try
            {
                var inventory = await _inventoryService.AdjustStockAsync(dto.ProductId, dto.BranchId, dto.NewQuantity);
                
                return Json(new { 
                    success = true, 
                    message = "Stock actualizado exitosamente",
                    newQuantity = inventory.Quantity
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al actualizar stock: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al actualizar stock: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStockHistory(Guid productId, Guid branchId)
        {
            try
            {
                var history = await _inventoryService.GetStockHistoryAsync(productId, branchId);
                
                // Por ahora retornamos una lista vacía ya que no tenemos tabla de historial
                var historyData = new List<object>();

                return Json(new { 
                    success = true, 
                    history = historyData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener historial: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener historial: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SyncAllStock()
        {
            try
            {
                Console.WriteLine($"[InventoryController] 🔄 SyncAllStock iniciado");
                Console.WriteLine($"[InventoryController] Llamando a SyncAllProductsStockAsync...");
                
                await _inventoryService.SyncAllProductsStockAsync();
                
                Console.WriteLine($"[InventoryController] ✅ SyncAllProductsStockAsync completado exitosamente");
                
                return Json(new { 
                    success = true, 
                    message = "Sincronización de stock completada exitosamente"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] ❌ Error en SyncAllStock: {ex.Message}");
                Console.WriteLine($"[InventoryController] Stack trace: {ex.StackTrace}");
                return Json(new { 
                    success = false, 
                    message = $"Error en sincronización: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabaseData()
        {
            try
            {
                Console.WriteLine($"[InventoryController] CheckDatabaseData iniciado");
                
                // Verificar productos
                var products = await _productService.GetAllAsync();
                Console.WriteLine($"[InventoryController] Productos en BD: {products.Count()}");
                
                // Verificar categorías
                var categories = await _categoryService.GetAllCategoriesAsync();
                Console.WriteLine($"[InventoryController] Categorías en BD: {categories.Count()}");
                
                // Verificar sucursales
                var branches = await _branchService.GetAllAsync();
                Console.WriteLine($"[InventoryController] Sucursales en BD: {branches.Count()}");
                
                // Verificar inventario
                var inventories = await _inventoryService.GetAllAsync();
                Console.WriteLine($"[InventoryController] Inventarios en BD: {inventories.Count()}");

                return Json(new { 
                    success = true, 
                    data = new {
                        productsCount = products.Count(),
                        categoriesCount = categories.Count(),
                        branchesCount = branches.Count(),
                        inventoriesCount = inventories.Count()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al verificar datos: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al verificar datos: {ex.Message}" 
                });
            }
        }
    }

    // ViewModel para la vista de inventario
    public class InventoryViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Inventory> LowStockItems { get; set; } = new List<Inventory>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
    }

    // DTOs para operaciones de inventario
    public class UpdateStockDto
    {
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal NewQuantity { get; set; }
        public string? Reason { get; set; }
    }
} 