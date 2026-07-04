using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Controllers;

[Authorize(Policy = "InventoryAccess")]
public class InventoryMovementController : Controller
{
    private readonly RestBarContext _context;
    private readonly IInventoryOperationsService _inventoryOps;
    private readonly IProductService _productService;

    public InventoryMovementController(RestBarContext context, IInventoryOperationsService inventoryOps, IProductService productService)
    {
        _context = context;
        _inventoryOps = inventoryOps;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMovementsByDateRange(DateTime? startDate, DateTime? endDate, Guid? branchId)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        var query = _context.InventoryMovements
            .Include(m => m.Product)
            .Include(m => m.Station)
            .Include(m => m.User)
            .Where(m => m.CreatedAt >= start && m.CreatedAt <= end);

        if (branchId.HasValue)
            query = query.Where(m => m.BranchId == branchId);

        var movements = await query.OrderByDescending(m => m.CreatedAt).Take(500).ToListAsync();

        return Json(new
        {
            success = true,
            movements = movements.Select(m => new
            {
                m.Id,
                m.ProductId,
                productName = m.Product?.Name,
                m.StationId,
                stationName = m.Station?.Name,
                movementType = m.MovementType.ToString(),
                m.Quantity,
                m.StockBefore,
                m.StockAfter,
                m.Reason,
                m.Reference,
                userName = m.User?.FullName,
                m.CreatedAt
            })
        });
    }

    public class MovementDto
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public Guid? StationId { get; set; }
        public Guid? BranchId { get; set; }
        public string? Reason { get; set; }
        public string? Reference { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePurchase([FromBody] MovementDto dto)
    {
        return await AdjustStock(dto, InventoryMovementType.Purchase, add: true);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdjustment([FromBody] MovementDto dto)
    {
        return await AdjustStock(dto, InventoryMovementType.Adjustment, add: dto.Quantity >= 0);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWaste([FromBody] MovementDto dto)
    {
        dto.Quantity = Math.Abs(dto.Quantity);
        return await AdjustStock(dto, InventoryMovementType.Waste, add: false);
    }

    private async Task<IActionResult> AdjustStock(MovementDto dto, InventoryMovementType type, bool add)
    {
        if (dto.ProductId == Guid.Empty || dto.Quantity == 0)
            return BadRequest(new { success = false, message = "Producto y cantidad requeridos" });

        var qty = Math.Abs(dto.Quantity);
        var before = await _productService.GetAvailableStockAsync(dto.ProductId, dto.BranchId);

        if (add)
            await _productService.RestoreStockAsync(dto.ProductId, qty, dto.StationId, dto.BranchId);
        else
            await _productService.ReduceStockAsync(dto.ProductId, qty, dto.StationId, dto.BranchId);

        var after = await _productService.GetAvailableStockAsync(dto.ProductId, dto.BranchId);
        var userId = Guid.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : (Guid?)null;

        await _inventoryOps.LogMovementAsync(dto.ProductId, type, add ? qty : -qty, before, after,
            dto.StationId, dto.BranchId, null, userId, null, dto.Reason, dto.Reference);

        return Json(new { success = true, stockAfter = after });
    }
}
