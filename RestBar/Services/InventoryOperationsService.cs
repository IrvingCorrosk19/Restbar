using Microsoft.EntityFrameworkCore;
using RestBar.Domain.FoodCost;
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

    /// <summary>
    /// Physical qty for one recipe line: base × portions × (1 + waste%) × (100 / yield%).
    /// Aligns POS consumption with FoodCost EffectiveIngredientQty + yield factor.
    /// </summary>
    public static decimal ComputeRecipeIngredientQty(decimal lineQty, decimal portions, decimal wastePercent, decimal yieldPercent)
    {
        var withWaste = FoodCostMath.EffectiveIngredientQty(lineQty * portions, wastePercent);
        if (yieldPercent <= 0m || yieldPercent == 100m)
            return Math.Round(withWaste, 4);
        return Math.Round(withWaste * (100m / yieldPercent), 4);
    }

    public async Task DeductInventoryForSaleAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId)
    {
        var recipe = await _context.Recipes.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        if (recipe != null && recipe.Lines.Count > 0)
        {
            var notifications = new List<(Guid ProductId, Guid? StationId, Guid? BranchId, decimal After, decimal Before, decimal Qty)>();
            foreach (var line in recipe.Lines)
            {
                var ingredientQty = ComputeRecipeIngredientQty(line.Quantity, quantity, line.WastePercent, recipe.YieldPercent);
                var ingredientStation = line.StationId ?? stationId;
                var stockBefore = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                await _productService.ReduceStockAsync(line.IngredientProductId, ingredientQty, ingredientStation, branchId, persist: false);
                var stockAfter = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                AddMovement(line.IngredientProductId, InventoryMovementType.Sale, -ingredientQty, stockBefore, stockAfter, ingredientStation, branchId, companyId, userId, orderId, $"Consumo receta {recipe.Name} (waste/yield)", productId.ToString());
                notifications.Add((line.IngredientProductId, ingredientStation, branchId, stockAfter, stockBefore, ingredientQty));
            }
            await _context.SaveChangesAsync();
            foreach (var n in notifications)
                await NotifyStock(n.ProductId, n.StationId, n.BranchId, n.After, n.Before, n.Qty);
            _logger.LogInformation("[InventoryOps] Receta aplicada para producto {ProductId}, {LineCount} ingredientes (waste/yield)", productId, recipe.Lines.Count);
            return;
        }

        var before = await _productService.GetAvailableStockAsync(productId, branchId);
        await _productService.ReduceStockAsync(productId, quantity, stationId, branchId, persist: false);
        var after = await _productService.GetAvailableStockAsync(productId, branchId);
        AddMovement(productId, InventoryMovementType.Sale, -quantity, before, after, stationId, branchId, companyId, userId, orderId, "Venta directa", null);
        await _context.SaveChangesAsync();
        await NotifyStock(productId, stationId, branchId, after, before, quantity);
    }

    public async Task RestoreInventoryForCancelAsync(Guid productId, decimal quantity, Guid? stationId, Guid? branchId, Guid? companyId, Guid? orderId, Guid? userId)
    {
        var recipe = await _context.Recipes.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive);

        if (recipe != null && recipe.Lines.Count > 0)
        {
            var notifications = new List<(Guid ProductId, Guid? StationId, Guid? BranchId, decimal After, decimal Before, decimal Qty)>();
            foreach (var line in recipe.Lines)
            {
                var ingredientQty = ComputeRecipeIngredientQty(line.Quantity, quantity, line.WastePercent, recipe.YieldPercent);
                var ingredientStation = line.StationId ?? stationId;
                var stockBefore = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                await _productService.RestoreStockAsync(line.IngredientProductId, ingredientQty, ingredientStation, branchId, persist: false);
                var stockAfter = await _productService.GetStockInStationAsync(line.IngredientProductId, ingredientStation ?? Guid.Empty, branchId);
                AddMovement(line.IngredientProductId, InventoryMovementType.CancelRestore, ingredientQty, stockBefore, stockAfter, ingredientStation, branchId, companyId, userId, orderId, $"Cancel consumo receta {recipe.Name}", productId.ToString());
                notifications.Add((line.IngredientProductId, ingredientStation, branchId, stockAfter, stockBefore, ingredientQty));
            }
            await _context.SaveChangesAsync();
            foreach (var n in notifications)
                await NotifyStock(n.ProductId, n.StationId, n.BranchId, n.After, n.Before, n.Qty);
            return;
        }

        var before = await _productService.GetAvailableStockAsync(productId, branchId);
        await _productService.RestoreStockAsync(productId, quantity, stationId, branchId, persist: false);
        var after = await _productService.GetAvailableStockAsync(productId, branchId);
        AddMovement(productId, InventoryMovementType.CancelRestore, quantity, before, after, stationId, branchId, companyId, userId, orderId, "Cancelación", null);
        await _context.SaveChangesAsync();
        await NotifyStock(productId, stationId, branchId, after, before, quantity);
    }

    public async Task TransferStockBetweenStationsAsync(Guid productId, Guid fromStationId, Guid toStationId, decimal quantity, Guid? branchId, Guid? companyId, Guid? userId, string? reason = null)
    {
        if (quantity <= 0) throw new ArgumentException("Cantidad debe ser mayor a 0");
        if (fromStationId == toStationId) throw new InvalidOperationException("Estaciones origen y destino deben ser distintas");

        var stockFrom = await _productService.GetStockInStationAsync(productId, fromStationId, branchId);
        if (stockFrom < quantity)
            throw new InvalidOperationException($"Stock insuficiente en estación origen. Disponible: {stockFrom}");

        await _productService.ReduceStockAsync(productId, quantity, fromStationId, branchId, persist: false);
        var fromAfter = await _productService.GetStockInStationAsync(productId, fromStationId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.TransferOut, -quantity, stockFrom, fromAfter, fromStationId, branchId, companyId, userId, null, reason, toStationId.ToString());

        var stockTo = await _productService.GetStockInStationAsync(productId, toStationId, branchId);
        await _productService.RestoreStockAsync(productId, quantity, toStationId, branchId, persist: false);
        var toAfter = await _productService.GetStockInStationAsync(productId, toStationId, branchId);
        await LogMovementAsync(productId, InventoryMovementType.TransferIn, quantity, stockTo, toAfter, toStationId, branchId, companyId, userId, null, reason, fromStationId.ToString());

        await NotifyStock(productId, fromStationId, branchId, fromAfter, stockFrom, quantity);
        await NotifyStock(productId, toStationId, branchId, toAfter, stockTo, quantity);
    }

    public async Task<InventoryMovement> LogMovementAsync(Guid productId, InventoryMovementType type, decimal quantity, decimal stockBefore, decimal stockAfter, Guid? stationId, Guid? branchId, Guid? companyId, Guid? userId, Guid? orderId, string? reason, string? reference)
    {
        var movement = AddMovement(productId, type, quantity, stockBefore, stockAfter, stationId, branchId, companyId, userId, orderId, reason, reference);
        await _context.SaveChangesAsync();
        return movement;
    }

    private InventoryMovement AddMovement(Guid productId, InventoryMovementType type, decimal quantity, decimal stockBefore, decimal stockAfter, Guid? stationId, Guid? branchId, Guid? companyId, Guid? userId, Guid? orderId, string? reason, string? reference)
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
