using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RestBar.Interfaces;
using RestBar.Models;
using System.Security.Claims;

namespace RestBar.Controllers
{
    [Authorize(Policy = "InventoryAccess")]
    public class InventoryMovementController : Controller
    {
        private readonly IInventoryMovementService _movementService;
        private readonly IInventoryService _inventoryService;
        private readonly IProductService _productService;
        private readonly IBranchService _branchService;
        private readonly IUserService _userService;

        public InventoryMovementController(
            IInventoryMovementService movementService,
            IInventoryService inventoryService,
            IProductService productService,
            IBranchService branchService,
            IUserService userService)
        {
            _movementService = movementService;
            _inventoryService = inventoryService;
            _productService = productService;
            _branchService = branchService;
            _userService = userService;
        }

        // GET: InventoryMovement
        public async Task<IActionResult> Index()
        {
            try
            {
                var movements = await _movementService.GetAllAsync();
                var products = await _productService.GetAllAsync();
                var branches = await _branchService.GetAllAsync();

                var viewModel = new InventoryMovementViewModel
                {
                    Movements = movements,
                    Products = products,
                    Branches = branches
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error en Index: {ex.Message}");
                return View("Error");
            }
        }

        // GET: InventoryMovement/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (!id.HasValue)
                return NotFound();

            var movement = await _movementService.GetByIdAsync(id.Value);
            if (movement == null)
                return NotFound();

            return View(movement);
        }

        // GET: InventoryMovement/ByProduct/5
        public async Task<IActionResult> ByProduct(Guid productId)
        {
            try
            {
                var movements = await _movementService.GetMovementsByProductAsync(productId);
                var product = await _productService.GetByIdAsync(productId);

                var viewModel = new ProductMovementViewModel
                {
                    Movements = movements,
                    Product = product
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error en ByProduct: {ex.Message}");
                return View("Error");
            }
        }

        // GET: InventoryMovement/ByBranch/5
        public async Task<IActionResult> ByBranch(Guid branchId)
        {
            try
            {
                var movements = await _movementService.GetMovementsByBranchAsync(branchId);
                var branch = await _branchService.GetByIdAsync(branchId);

                var viewModel = new BranchMovementViewModel
                {
                    Movements = movements,
                    Branch = branch
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error en ByBranch: {ex.Message}");
                return View("Error");
            }
        }

        // POST: InventoryMovement/CreateAdjustment
        [HttpPost]
        public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentDto dto)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                {
                    return Json(new { success = false, message = "Usuario no válido" });
                }

                var movement = await _movementService.CreateAdjustmentMovementAsync(
                    dto.InventoryId,
                    dto.ProductId,
                    dto.BranchId,
                    userIdGuid,
                    dto.Quantity,
                    dto.Reason
                );

                return Json(new { 
                    success = true, 
                    message = "Ajuste creado exitosamente",
                    movementId = movement.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al crear ajuste: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear ajuste: {ex.Message}" 
                });
            }
        }

        // POST: InventoryMovement/CreatePurchase
        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseDto dto)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                {
                    return Json(new { success = false, message = "Usuario no válido" });
                }

                var movement = await _movementService.CreatePurchaseMovementAsync(
                    dto.InventoryId,
                    dto.ProductId,
                    dto.BranchId,
                    userIdGuid,
                    dto.Quantity,
                    dto.UnitCost,
                    dto.Reason
                );

                return Json(new { 
                    success = true, 
                    message = "Compra registrada exitosamente",
                    movementId = movement.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al crear compra: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al registrar compra: {ex.Message}" 
                });
            }
        }

        // POST: InventoryMovement/CreateWaste
        [HttpPost]
        public async Task<IActionResult> CreateWaste([FromBody] CreateWasteDto dto)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                {
                    return Json(new { success = false, message = "Usuario no válido" });
                }

                var movement = await _movementService.CreateWasteMovementAsync(
                    dto.InventoryId,
                    dto.ProductId,
                    dto.BranchId,
                    userIdGuid,
                    dto.Quantity,
                    dto.Reason
                );

                return Json(new { 
                    success = true, 
                    message = "Pérdida registrada exitosamente",
                    movementId = movement.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al crear pérdida: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al registrar pérdida: {ex.Message}" 
                });
            }
        }

        // POST: InventoryMovement/CreateTransfer
        [HttpPost]
        public async Task<IActionResult> CreateTransfer([FromBody] CreateTransferDto dto)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                {
                    return Json(new { success = false, message = "Usuario no válido" });
                }

                var movement = await _movementService.CreateTransferMovementAsync(
                    dto.FromInventoryId,
                    dto.ToInventoryId,
                    dto.ProductId,
                    dto.FromBranchId,
                    dto.ToBranchId,
                    userIdGuid,
                    dto.Quantity,
                    dto.Reason
                );

                return Json(new { 
                    success = true, 
                    message = "Transferencia creada exitosamente",
                    movementId = movement.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al crear transferencia: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al crear transferencia: {ex.Message}" 
                });
            }
        }

        // GET: InventoryMovement/GetMovementsByDateRange
        [HttpGet]
        public async Task<IActionResult> GetMovementsByDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                var movements = await _movementService.GetMovementsByDateRangeAsync(startDate, endDate);
                
                var movementsData = movements.Select(m => new
                {
                    m.Id,
                    m.Type,
                    m.Quantity,
                    m.PreviousStock,
                    m.NewStock,
                    m.Reason,
                    m.Reference,
                    m.CreatedAt,
                    ProductName = m.Product?.Name,
                    BranchName = m.Branch?.Name,
                    UserName = m.User?.FullName
                });

                return Json(new { 
                    success = true, 
                    movements = movementsData
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al obtener movimientos: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener movimientos: {ex.Message}" 
                });
            }
        }

        // GET: InventoryMovement/GetMovementSummary
        [HttpGet]
        public async Task<IActionResult> GetMovementSummary(DateTime startDate, DateTime endDate)
        {
            try
            {
                var summary = await _movementService.GetMovementSummaryAsync(startDate, endDate);
                
                return Json(new { 
                    success = true, 
                    summary = summary
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al obtener resumen: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener resumen: {ex.Message}" 
                });
            }
        }

        // GET: InventoryMovement/GetProductHistory
        [HttpGet]
        public async Task<IActionResult> GetProductHistory(Guid productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var history = await _movementService.GetProductMovementHistoryAsync(productId, startDate, endDate);
                
                return Json(new { 
                    success = true, 
                    history = history
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[InventoryMovementController] Error al obtener historial: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = $"Error al obtener historial: {ex.Message}" 
                });
            }
        }
    }

    // ViewModels
    public class InventoryMovementViewModel
    {
        public IEnumerable<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Branch> Branches { get; set; } = new List<Branch>();
    }

    public class ProductMovementViewModel
    {
        public IEnumerable<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
        public Product? Product { get; set; }
    }

    public class BranchMovementViewModel
    {
        public IEnumerable<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();
        public Branch? Branch { get; set; }
    }

    // DTOs
    public class CreateAdjustmentDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Quantity { get; set; }
        public string Reason { get; set; } = "";
    }

    public class CreatePurchaseDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? Reason { get; set; }
    }

    public class CreateWasteDto
    {
        public Guid InventoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid BranchId { get; set; }
        public decimal Quantity { get; set; }
        public string Reason { get; set; } = "";
    }

    public class CreateTransferDto
    {
        public Guid FromInventoryId { get; set; }
        public Guid ToInventoryId { get; set; }
        public Guid ProductId { get; set; }
        public Guid FromBranchId { get; set; }
        public Guid ToBranchId { get; set; }
        public decimal Quantity { get; set; }
        public string? Reason { get; set; }
    }
} 