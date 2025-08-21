using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;
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
        private readonly IInventoryMovementService _movementService;
        private readonly IUserService _userService;

        public InventoryController(
            IInventoryService inventoryService,
            IProductService productService,
            IBranchService branchService,
            ICategoryService categoryService,
            IInventoryMovementService movementService,
            IUserService userService)
        {
            _inventoryService = inventoryService;
            _productService = productService;
            _branchService = branchService;
            _categoryService = categoryService;
            _movementService = movementService;
            _userService = userService;
        }

        private Guid? GetCurrentUserCompanyId()
        {
            var companyIdClaim = User.FindFirst("CompanyId");
            if (companyIdClaim != null && Guid.TryParse(companyIdClaim.Value, out Guid companyId))
            {
                return companyId;
            }
            return null;
        }

        private async Task<Guid?> GetUserCompanyIdAsync()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
                return null;

            var user = await _userService.GetByEmailAsync(userEmail);
            return user?.Branch?.CompanyId;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return View("Error", "No se pudo determinar la compañía del usuario");
                }

                // Obtener productos de la compañía actual
                var products = await _productService.GetAllAsync();
                
                // Obtener items de bajo stock de la compañía actual
                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                
                // Obtener sucursales de la compañía actual
                var branches = await _branchService.GetByCompanyIdAsync(companyId!.Value);

                var viewModel = new InventoryViewModel
                {
                    Products = products,
                    LowStockItems = lowStockItems,
                    Branches = branches,
                    CompanyId = companyId.Value
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
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(branchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

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
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(branchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

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
        public async Task<IActionResult> GetByBranchAndProduct(Guid productId, Guid branchId)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(branchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

                var inventory = await _inventoryService.GetByBranchAndProductAsync(branchId, productId);
                
                if (inventory == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "No se encontró inventario para este producto y sucursal" 
                    });
                }

                return Json(new { 
                    success = true, 
                    inventory = new
                    {
                        id = inventory.Id,
                        productId = inventory.ProductId,
                        productName = inventory.Product?.Name,
                        branchId = inventory.BranchId,
                        branchName = inventory.Branch?.Name,
                        quantity = inventory.Quantity,
                        unit = inventory.Unit,
                        minThreshold = inventory.MinThreshold,
                        lastUpdated = inventory.LastUpdated,
                        stock = inventory.Stock,
                        minStock = inventory.MinStock,
                        maxStock = inventory.MaxStock
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener inventario: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener inventario: {ex.Message}" 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLowStockReport()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var lowStockItems = await _inventoryService.GetLowStockItemsAsync();
                
                // Filtrar por compañía
                var companyBranches = await _branchService.GetByCompanyIdAsync(companyId.Value);
                var companyBranchIds = companyBranches.Select(b => b.Id).ToHashSet();
                
                var filteredItems = lowStockItems.Where(item => item.BranchId.HasValue && companyBranchIds.Contains(item.BranchId.Value));
                
                var result = filteredItems.Select(item => new
                {
                    id = item.Id,
                    productId = item.ProductId,
                    productName = item.Product?.Name,
                    productDescription = item.Product?.Description,
                    categoryName = item.Product?.Category?.Name,
                    branchId = item.BranchId,
                    branchName = item.Branch?.Name,
                    quantity = item.Quantity,
                    unit = item.Unit,
                    minThreshold = item.MinThreshold,
                    lastUpdated = item.LastUpdated,
                    stock = item.Stock,
                    minStock = item.MinStock,
                    maxStock = item.MaxStock
                });

                return Json(new { 
                    success = true, 
                    lowStockItems = result 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener reporte de stock bajo: {ex.Message}");
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
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var inventoryData = await _inventoryService.GetAllAsync();
                
                // Filtrar por compañía
                var companyBranches = await _branchService.GetByCompanyIdAsync(companyId.Value);
                var companyBranchIds = companyBranches.Select(b => b.Id).ToHashSet();
                
                var filteredInventory = inventoryData.Where(i => i.BranchId.HasValue && companyBranchIds.Contains(i.BranchId.Value));
                
                var result = filteredInventory.Select(i => new
                {
                    id = i.Id,
                    productId = i.ProductId,
                    productName = i.Product?.Name,
                    productDescription = i.Product?.Description,
                    categoryName = i.Product?.Category?.Name,
                    branchId = i.BranchId,
                    branchName = i.Branch?.Name,
                    quantity = i.Quantity,
                    unit = i.Unit,
                    minStock = i.MinThreshold,
                    maxStock = i.MaxStock,
                    reorderPoint = i.ReorderPoint,
                    reorderQuantity = i.ReorderQuantity,
                    location = i.Location,
                    barcode = i.Barcode,
                    unitCost = i.UnitCost,
                    totalValue = i.TotalValue,
                    notes = i.Notes,
                    lastUpdated = i.LastUpdated,
                    isActive = i.IsActive
                });

                return Json(new { 
                    success = true, 
                    inventory = result 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener datos de inventario: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener datos: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock([FromBody] UpdateStockDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(dto.BranchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

                var inventory = await _inventoryService.AdjustStockAsync(dto.ProductId, dto.BranchId, dto.NewQuantity);
                
                // Obtener el usuario actual
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userService.GetByEmailAsync(userEmail ?? "");
                var userId = user?.Id ?? Guid.Empty;

                // Crear movimiento de ajuste
                await _movementService.CreateAdjustmentMovementAsync(
                    inventory.Id,
                    dto.ProductId,
                    dto.BranchId,
                    userId,
                    dto.NewQuantity - inventory.Quantity, // Diferencia
                    dto.Reason ?? "Ajuste manual"
                );
                
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
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(branchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

                // Obtener movimientos por producto y sucursal
                var movements = await _movementService.GetMovementsByProductAsync(productId);
                
                // Filtrar por sucursal
                var filteredMovements = movements.Where(m => m.BranchId == branchId);
                
                var result = filteredMovements.Select(m => new
                {
                    id = m.Id,
                    type = m.Type.ToString(),
                    quantity = m.Quantity,
                    previousStock = m.PreviousStock,
                    newStock = m.NewStock,
                    reason = m.Reason,
                    reference = m.Reference,
                    createdAt = m.CreatedAt,
                    userId = m.UserId,
                    userName = m.User?.FullName
                });

                return Json(new { 
                    success = true, 
                    movements = result 
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

        [HttpPost]
        public async Task<IActionResult> CreateMovement([FromBody] CreateMovementDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(dto.BranchId);
                if (branch?.CompanyId != companyId)
                {
                    return Json(new { success = false, message = "No tienes permisos para esta sucursal" });
                }

                var inventory = await _inventoryService.GetByBranchAndProductAsync(dto.BranchId, dto.ProductId);
                
                if (inventory == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "No se encontró inventario para este producto y sucursal" 
                    });
                }

                // Obtener el usuario actual
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userService.GetByEmailAsync(userEmail ?? "");
                var userId = user?.Id ?? Guid.Empty;

                // Crear movimiento según el tipo
                switch (dto.MovementType)
                {
                    case "purchase":
                        await _movementService.CreatePurchaseMovementAsync(
                            inventory.Id, dto.ProductId, dto.BranchId, userId, dto.Quantity, dto.UnitCost, dto.Reason);
                        break;
                    case "sale":
                        await _movementService.CreateSaleMovementAsync(
                            inventory.Id, dto.ProductId, dto.BranchId, userId, dto.Quantity, dto.Reason);
                        break;
                    case "adjustment":
                        await _movementService.CreateAdjustmentMovementAsync(
                            inventory.Id, dto.ProductId, dto.BranchId, userId, dto.Quantity, dto.Reason ?? "Ajuste manual");
                        break;
                    case "transfer":
                        // Para transferencias necesitamos dos sucursales
                        return Json(new { 
                            success = false, 
                            message = "Las transferencias requieren especificar sucursal origen y destino" 
                        });
                    case "waste":
                        await _movementService.CreateWasteMovementAsync(
                            inventory.Id, dto.ProductId, dto.BranchId, userId, dto.Quantity, dto.Reason ?? "Pérdida");
                        break;
                    default:
                        return Json(new { 
                            success = false, 
                            message = "Tipo de movimiento no válido" 
                        });
                }
                
                return Json(new { 
                    success = true, 
                    message = "Movimiento creado exitosamente"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al crear movimiento: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear movimiento: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Cost = dto.Cost,
                    TaxRate = dto.TaxRate,
                    Unit = dto.Unit,
                    CategoryId = dto.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdProduct = await _productService.CreateAsync(product);
                
                return Json(new { 
                    success = true, 
                    message = "Producto creado exitosamente",
                    product = new
                    {
                        id = createdProduct.Id,
                        name = createdProduct.Name,
                        description = createdProduct.Description,
                        price = createdProduct.Price,
                        cost = createdProduct.Cost,
                        unit = createdProduct.Unit,
                        categoryId = createdProduct.CategoryId,
                        categoryName = createdProduct.Category?.Name
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al crear producto: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear producto: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    IsActive = true
                };

                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                
                return Json(new { 
                    success = true, 
                    message = "Categoría creada exitosamente",
                    category = new
                    {
                        id = createdCategory.Id,
                        name = createdCategory.Name,
                        description = createdCategory.Description
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al crear categoría: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear categoría: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var branch = new Branch
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId.Value, // Asignar a la compañía del usuario
                    Name = dto.Name,
                    Address = dto.Address,
                    Phone = dto.Phone,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdBranch = await _branchService.CreateAsync(branch);
                
                return Json(new { 
                    success = true, 
                    message = "Sucursal creada exitosamente",
                    branch = new
                    {
                        id = createdBranch.Id,
                        name = createdBranch.Name,
                        address = createdBranch.Address,
                        phone = createdBranch.Phone,
                        companyId = createdBranch.CompanyId
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al crear sucursal: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear sucursal: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddInventory([FromBody] AddInventoryDto dto)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                // Verificar que la sucursal pertenece a la compañía del usuario
                var branch = await _branchService.GetByIdAsync(dto.BranchId);
                if (branch == null || branch.CompanyId != companyId.Value)
                {
                    return Json(new { success = false, message = "Sucursal no válida" });
                }

                // Verificar que el producto existe
                var product = await _productService.GetByIdAsync(dto.ProductId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Crear inventario inicial
                var inventory = new Inventory
                {
                    ProductId = dto.ProductId,
                    BranchId = dto.BranchId,
                    Quantity = dto.InitialQuantity,
                    MinStock = (int?)dto.MinThreshold,
                    MaxStock = (int?)dto.MaxStock,
                    ReorderPoint = (int?)dto.ReorderPoint,
                    ReorderQuantity = (int?)dto.ReorderQuantity,
                    UnitCost = dto.UnitCost,
                    TotalValue = dto.InitialQuantity * dto.UnitCost,
                    IsActive = true,
                    LastUpdated = DateTime.UtcNow
                };

                await _inventoryService.CreateAsync(inventory);

                // Crear movimiento inicial
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userService.GetByEmailAsync(userEmail ?? "");
                var userId = user?.Id ?? Guid.Empty;

                await _movementService.CreateAdjustmentMovementAsync(
                    inventory.Id, dto.ProductId, dto.BranchId, userId, dto.InitialQuantity, "Inventario inicial");

                return Json(new { success = true, message = "Inventario creado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // Métodos para obtener datos para los modales
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var products = await _productService.GetAllAsync();
                // Filtrar productos por compañía (si es necesario)
                var filteredProducts = products.ToList(); // Por ahora no filtramos por compañía ya que Category no tiene CompanyId

                return Json(new { success = true, products = filteredProducts });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var categories = await _categoryService.GetAllCategoriesAsync();
                // Por ahora no filtramos por compañía ya que Category no tiene CompanyId
                var filteredCategories = categories.ToList();

                return Json(new { success = true, categories = filteredCategories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var branches = await _branchService.GetByCompanyIdAsync(companyId.Value);

                return Json(new { success = true, branches = branches });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDatabaseData()
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var products = await _productService.GetAllAsync();
                var categories = await _categoryService.GetAllCategoriesAsync();
                var branches = await _branchService.GetByCompanyIdAsync(companyId!.Value);
                var inventory = await _inventoryService.GetAllAsync();
                
                // Filtrar inventario por compañía
                var companyBranchIds = branches.Select(b => b.Id).ToHashSet();
                var companyInventory = inventory.Where(i => i.BranchId.HasValue && companyBranchIds.Contains(i.BranchId.Value));
                
                return Json(new { 
                    success = true, 
                    data = new
                    {
                        productsCount = products.Count(),
                        categoriesCount = categories.Count(),
                        branchesCount = branches.Count(),
                        inventoryCount = companyInventory.Count(),
                        companyId = companyId.Value
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

        [HttpGet]
        public async Task<IActionResult> GetMovementHistory(Guid productId, Guid branchId)
        {
            try
            {
                var movements = await _movementService.GetMovementsByProductAsync(productId);
                var filteredMovements = movements.Where(m => m.BranchId == branchId);
                
                var result = filteredMovements.Select(m => new
                {
                    id = m.Id,
                    type = m.Type.ToString(),
                    quantity = m.Quantity,
                    previousStock = m.PreviousStock,
                    newStock = m.NewStock,
                    reason = m.Reason,
                    reference = m.Reference,
                    createdAt = m.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    userName = m.User?.FullName ?? "Sistema"
                });

                return Json(new { success = true, movements = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener historial de movimientos: {ex.Message}");
                return Json(new { success = false, message = $"Error al obtener historial: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovementSummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var summary = await _movementService.GetMovementSummaryAsync(startDate, endDate);
                
                // Filtrar por compañía
                var companyBranches = await _branchService.GetByCompanyIdAsync(companyId.Value);
                var companyBranchIds = companyBranches.Select(b => b.Id).ToHashSet();
                
                var filteredSummary = summary.Where(s => 
                {
                    var branchId = s.GetType().GetProperty("BranchId")?.GetValue(s);
                    return branchId != null && companyBranchIds.Contains((Guid)branchId);
                });

                return Json(new { success = true, summary = filteredSummary });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener resumen de movimientos: {ex.Message}");
                return Json(new { success = false, message = $"Error al obtener resumen: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMovementsByType(string movementType)
        {
            try
            {
                if (!Enum.TryParse<MovementType>(movementType, true, out var type))
                {
                    return Json(new { success = false, message = "Tipo de movimiento inválido" });
                }

                var companyId = await GetUserCompanyIdAsync();
                if (companyId == null)
                {
                    return Json(new { success = false, message = "No se pudo determinar la compañía del usuario" });
                }

                var movements = await _movementService.GetMovementsByTypeAsync(type);
                
                // Filtrar por compañía
                var companyBranches = await _branchService.GetByCompanyIdAsync(companyId.Value);
                var companyBranchIds = companyBranches.Select(b => b.Id).ToHashSet();
                
                var filteredMovements = movements.Where(m => companyBranchIds.Contains(m.BranchId));
                
                var result = filteredMovements.Select(m => new
                {
                    id = m.Id,
                    productName = m.Product?.Name,
                    branchName = m.Branch?.Name,
                    type = m.Type.ToString(),
                    quantity = m.Quantity,
                    reason = m.Reason,
                    reference = m.Reference,
                    createdAt = m.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    userName = m.User?.FullName ?? "Sistema"
                });

                return Json(new { success = true, movements = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryController] Error al obtener movimientos por tipo: {ex.Message}");
                return Json(new { success = false, message = $"Error al obtener movimientos: {ex.Message}" });
            }
        }
    }

    public class InventoryViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Inventory> LowStockItems { get; set; } = new List<Inventory>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
        public Guid CompanyId { get; set; }
    }

    public class UpdateStockDto
    {
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal NewQuantity { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateMovementDto
    {
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Quantity { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Reference { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal TaxRate { get; set; }
        public string Unit { get; set; } = string.Empty;
        public Guid? CategoryId { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class AddInventoryDto
    {
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal InitialQuantity { get; set; }
        public decimal MinThreshold { get; set; }
        public decimal MaxStock { get; set; }
        public decimal ReorderPoint { get; set; }
        public decimal ReorderQuantity { get; set; }
        public decimal UnitCost { get; set; }
    }
} 