using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services;

public class InventoryOperationsService : IInventoryOperationsService
{
    private readonly RestBarContext _context;
    private readonly IProductService _productService;
    private readonly IOrderHubService _orderHubService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InventoryOperationsService> _logger;

    public InventoryOperationsService(
        RestBarContext context,
        IProductService productService,
        IOrderHubService orderHubService,
        IConfiguration configuration,
        ILogger<InventoryOperationsService> logger)
    {
        _context = context;
        _productService = productService;
        _orderHubService = orderHubService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task DeductInventoryForSaleAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        if (recipe != null && recipe.Lines.Count > 0)
        {
            foreach (var line in recipe.Lines)
            {
                var ingredientQty = line.Quantity * quantity;
                var ingredientStation = line.StationId ?? stationId;
                var stockBefore = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                await _productService.ReduceStockAsync(line.IngredientProductId, ingredientQty, ingredientStation, branchId);
                var stockAfter = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                await LogMovementAsync(line.IngredientProductId, InventoryMovementType.Sale, -ingredientQty, stockBefore, stockAfter, ingredientStation, branchId, companyId, userId, orderId, $"Receta {recipe.Name}", productId.ToString());
                await NotifyStock(line.IngredientProductId, ingredientStation, branchId, stockAfter, stockBefore, ingredientQty);
            }
            _logger.LogInformation("[InventoryOps] Receta aplicada para producto {ProductId}, {LineCount} ingredientes", productId, recipe.Lines.Count);
            return;
        }

        var before = await _productService.GetAvailableStockAsync(productId, branchId);
        await _productService.ReduceStockAsync(productId, quantity, stationId, branchId);
        var after = await _productService.GetAvailableStockAsync(productId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.Sale, -quantity, before, after, stationId, branchId, companyId, userId, orderId, "Venta directa", null);
        await NotifyStock(productId, stationId, branchId, after, before, quantity);
    }

    public async Task RestoreInventoryForCancelAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        if (recipe != null && recipe.Lines.Count > 0)
        {
            foreach (var line in recipe.Lines)
            {
                var ingredientQty = line.Quantity * quantity;
                var ingredientStation = line.StationId ?? stationId;
                var stockBefore = await _productService.GetAvailableStockAsync(line.IngredientProductId, branchId);
                await _productService.RestoreStockAsync(line.IngredientProductId, ingredientQty, ingredientStation, branchId);
                var stockAfter = await _productService.GetAvailableStockAsync(line.IngredientProductId, branchId);
                await LogMovementAsync(line.IngredientProductId, InventoryMovementType.CancelRestore, ingredientQty, stockBefore, stockAfter, ingredientStation, branchId, companyId, userId, orderId, $"Cancel receta {recipe.Name}", productId.ToString());
                await NotifyStock(line.IngredientProductId, ingredientStation, branchId, stockAfter, stockBefore, ingredientQty);
            }
            return;
        }

        var before = await _productService.GetAvailableStockAsync(productId, branchId);
        await _productService.RestoreStockAsync(productId, quantity, stationId, branchId);
        var after = await _productService.GetAvailableStockAsync(productId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.CancelRestore, quantity, before, after, stationId, branchId, companyId, userId, orderId, "Cancelación", null);
        await NotifyStock(productId, stationId, branchId, after, before, quantity);
    }

    public async Task TransferStockBetweenStationsAsync(Guid productId, Guid fromStationId, Guid toStationId, decimal quantity, Guid? branchId, Guid? companyId, Guid? userId, string? reason = null)
    {
        if (quantity <= 0) throw new ArgumentException("Cantidad debe ser mayor a 0");
        if (fromStationId == toStationId) throw new InvalidOperationException("Estaciones origen y destino deben ser distintas");

        var stockFrom = await _productService.GetStockInStationAsync(productId, fromStationId, branchId);
        if (stockFrom < quantity)
            throw new InvalidOperationException($"Stock insuficiente en estación origen. Disponible: {stockFrom}");

        await _productService.ReduceStockAsync(productId, quantity, fromStationId, branchId);
        var fromAfter = await _productService.GetStockInStationAsync(productId, fromStationId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.TransferOut, -quantity, stockFrom, fromAfter, fromStationId, branchId, companyId, userId, null, reason, toStationId.ToString());

        var stockTo = await _productService.GetStockInStationAsync(productId, toStationId, branchId);
        await _productService.RestoreStockAsync(productId, quantity, toStationId, branchId);
        var toAfter = await _productService.GetStockInStationAsync(productId, toStationId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.TransferIn, quantity, stockTo, toAfter, toStationId, branchId, companyId, userId, null, reason, fromStationId.ToString());

        await NotifyStock(productId, fromStationId, branchId, fromAfter, stockFrom, quantity);
        await NotifyStock(productId, toStationId, branchId, toAfter, stockTo, quantity);
    }

    public async Task<InventoryMovement> LogMovementAsync(Guid productId, InventoryMovementType type, decimal quantity, decimal stockBefore, decimal stockAfter, Guid? stationId, Guid? branchId, Guid? companyId, Guid? userId, Guid? orderId, string? reason, string? reference)
    {
        var movement = new InventoryMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            MovementType = type,
            Quantity = quantity,
            StockBefore = stockBefore,
            StockAfter = stockAfter,
            StationId = stationId,
            BranchId = branchId,
            CompanyId = companyId,
            UserId = userId,
            OrderId = orderId,
            Reason = reason,
            Reference = reference,
            CreatedAt = DateTime.UtcNow
        };
        _context.InventoryMovements.Add(movement);
        await _context.SaveChangesAsync();
        return movement;
    }

    public async Task AllocateTipsAsync(Guid paymentId, Guid orderId, decimal tipAmount)
    {
        if (tipAmount <= 0) return;

        var orderItems = await _context.OrderItems
            .Where(oi => oi.OrderId == orderId && oi.Status != OrderItemStatus.Cancelled)
            .ToListAsync();

        if (!orderItems.Any()) return;

        var totalLineRevenue = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice - oi.Discount);
        if (totalLineRevenue <= 0) return;

        foreach (var item in orderItems)
        {
            var userId = item.AddedByUserId;
            if (!userId.HasValue) continue;

            var lineRevenue = item.Quantity * item.UnitPrice - item.Discount;
            var pct = lineRevenue / totalLineRevenue;
            var tipShare = Math.Round(tipAmount * pct, 2);

            if (tipShare <= 0) continue;

            _context.TipAllocations.Add(new TipAllocation
            {
                Id = Guid.NewGuid(),
                PaymentId = paymentId,
                OrderId = orderId,
                UserId = userId.Value,
                Amount = tipShare,
                Percentage = pct,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetCommissionRateAsync(Guid? companyId, Guid? branchId, UserRole? role, Guid? stationId)
    {
        var rule = await _context.CommissionRules
            .Where(r => r.IsActive)
            .Where(r => r.CompanyId == null || r.CompanyId == companyId)
            .Where(r => r.BranchId == null || r.BranchId == branchId)
            .Where(r => r.Role == null || r.Role == role)
            .Where(r => r.StationId == null || r.StationId == stationId)
            .OrderByDescending(r => r.StationId != null)
            .ThenByDescending(r => r.Role != null)
            .ThenByDescending(r => r.BranchId != null)
            .FirstOrDefaultAsync();

        if (rule != null) return rule.Rate;
        return _configuration.GetValue<decimal>("RestBar:DefaultCommissionRate", 0.05m);
    }

    private async Task NotifyStock(Guid productId, Guid? stationId, Guid? branchId, decimal newStock, decimal oldStock, decimal qtyChanged)
    {
        try
        {
            var product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
                await _orderHubService.NotifyStockReduced(productId, product.Name, oldStock, newStock, Math.Abs(qtyChanged));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[InventoryOps] NotifyStockReduced falló para {ProductId}", productId);
        }
    }
}
