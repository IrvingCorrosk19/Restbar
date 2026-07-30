using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestBar.Domain.FoodCost;
using RestBar.Infrastructure.FoodCost;
using RestBar.Infrastructure.Foundation;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.FoodCost;

public class FoodCostIntegrityService : IFoodCostIntegrityService
{
    private readonly RestBarContext _db;
    public FoodCostIntegrityService(RestBarContext db) => _db = db;

    public async Task AppendAsync(Guid companyId, Guid branchId, Guid actorId, string eventType, string entityType, Guid? entityId, string? afterJson = null, CancellationToken ct = default)
    {
        var prev = await _db.FoodCostAuditEvents.AsNoTracking()
            .Where(e => e.CompanyId == companyId)
            .OrderByDescending(e => e.CreatedAtUtc)
            .Select(e => e.EventHash)
            .FirstOrDefaultAsync(ct);

        var evt = new FoodCostAuditEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BranchId = branchId,
            EntityType = entityType,
            EntityId = entityId,
            EventType = eventType,
            ActorUserId = actorId,
            AfterJson = afterJson,
            PreviousEventHash = prev,
            CreatedAtUtc = DateTime.UtcNow
        };
        evt.EventHash = FoodCostHashChainBuilder.Compute(evt, prev);
        _db.FoodCostAuditEvents.Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}

public class FoodCostEngine : IFoodCostEngine
{
    private readonly RestBarContext _db;
    private readonly IFoodCostIntegrityService _audit;
    public const decimal VarianceAlertThresholdPts = 2m;

    public FoodCostEngine(RestBarContext db, IFoodCostIntegrityService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<PlateCostResult> GetPlateCostAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        var recipe = await _db.Recipes.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive, ct);

        var lines = new List<IngredientCostLine>();
        decimal raw = 0;

        if (recipe != null && recipe.Lines.Count > 0)
        {
            var ingredientIds = recipe.Lines.Select(l => l.IngredientProductId).ToList();
            var ingredients = await _db.Products.AsNoTracking()
                .Where(p => ingredientIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var line in recipe.Lines)
            {
                ingredients.TryGetValue(line.IngredientProductId, out var ing);
                var unitCost = ing?.AverageCost ?? ing?.Cost ?? 0m;
                var qty = FoodCostMath.EffectiveIngredientQty(line.Quantity, line.WastePercent);
                var lineCost = Math.Round(qty * unitCost, 4);
                raw += lineCost;
                lines.Add(new IngredientCostLine(
                    line.IngredientProductId, ing?.Name ?? "?", line.Quantity, line.WastePercent, unitCost, lineCost));
            }

            raw = FoodCostMath.ApplyYield(raw, recipe.YieldPercent);
        }
        else
        {
            raw = product.AverageCost ?? product.Cost ?? 0m;
        }

        var price = product.Price;
        return new PlateCostResult(
            productId, raw, price,
            FoodCostMath.FoodCostPercent(raw, price),
            FoodCostMath.GrossMargin(price, raw),
            FoodCostMath.GrossMarginPercent(price, raw),
            lines);
    }

    public async Task<FoodCostPeriodResult> GetPeriodAnalysisAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var items = await _db.OrderItems.AsNoTracking()
            .Where(i => i.BranchId == branchId && i.CreatedAt >= from && i.CreatedAt < to &&
                        i.Status != OrderItemStatus.Cancelled)
            .Select(i => new { i.ProductId, i.Quantity, i.UnitPrice, i.Discount, i.TheoreticalUnitCost })
            .ToListAsync(ct);

        var sales = items.Sum(i => i.Quantity * i.UnitPrice - i.Discount);

        decimal theo = 0;
        foreach (var i in items)
        {
            if (i.TheoreticalUnitCost.HasValue)
                theo += i.TheoreticalUnitCost.Value * i.Quantity;
            else if (i.ProductId.HasValue)
            {
                var plate = await GetPlateCostAsync(i.ProductId.Value, ct);
                theo += plate.TheoreticalCost * i.Quantity;
            }
        }

        var movements = await _db.InventoryMovements.AsNoTracking()
            .Where(m => m.BranchId == branchId && m.CreatedAt >= from && m.CreatedAt < to &&
                        (m.MovementType == InventoryMovementType.Sale || m.MovementType == InventoryMovementType.Waste))
            .ToListAsync(ct);

        var productIds = movements.Select(m => m.ProductId).Distinct().ToList();
        var costs = await _db.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.AverageCost ?? p.Cost ?? 0m, ct);

        decimal actual = 0, waste = 0;
        foreach (var m in movements)
        {
            var unit = m.UnitCost ?? (costs.TryGetValue(m.ProductId, out var c) ? c : 0m);
            var amt = Math.Abs(m.Quantity) * unit;
            actual += amt;
            if (m.MovementType == InventoryMovementType.Waste)
                waste += amt;
        }

        // Prefer usage from sales snapshots when denser; blend: actual = max(movement-based, theo) no —
        // Actual from movements; if no unit costs, approximate with theo + waste events
        var wasteEvents = await _db.WasteEvents.AsNoTracking()
            .Where(w => w.BranchId == branchId && w.CreatedAt >= from && w.CreatedAt < to)
            .SumAsync(w => (decimal?)w.TotalCost, ct) ?? 0m;
        if (wasteEvents > waste) waste = wasteEvents;

        if (actual == 0 && theo > 0)
            actual = theo + waste; // fallback when movements lack cost

        var variance = FoodCostMath.VarianceAmount(actual, theo);
        var theoPct = FoodCostMath.PercentOfSales(theo, sales);
        var actPct = FoodCostMath.PercentOfSales(actual, sales);

        return new FoodCostPeriodResult(
            sales, theo, actual, waste, variance,
            FoodCostMath.VariancePoints(actPct, theoPct),
            theoPct, actPct,
            FoodCostMath.GrossMarginPercent(sales, theo));
    }

    public async Task<FoodCostSnapshot> GenerateSnapshotAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, Guid? userId, CancellationToken ct = default)
    {
        var r = await GetPeriodAnalysisAsync(companyId, branchId, from, to, ct);
        var snap = new FoodCostSnapshot
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BranchId = branchId,
            PeriodStart = from,
            PeriodEnd = to,
            SalesTotal = r.SalesTotal,
            TheoreticalCogs = r.TheoreticalCogs,
            ActualCogs = r.ActualCogs,
            VarianceAmount = r.VarianceAmount,
            VariancePercent = r.VariancePoints,
            WasteCost = r.WasteCost,
            FoodCostPercentTheo = r.TheoFoodCostPercent,
            FoodCostPercentActual = r.ActualFoodCostPercent,
            GeneratedByUserId = userId
        };
        _db.FoodCostSnapshots.Add(snap);

        if (Math.Abs(r.VariancePoints) >= VarianceAlertThresholdPts)
        {
            _db.VarianceAlerts.Add(new VarianceAlert
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                BranchId = branchId,
                AlertType = VarianceAlertType.OverUsage,
                Severity = Math.Abs(r.VariancePoints) >= 5 ? VarianceSeverity.High : VarianceSeverity.Medium,
                Message = $"Variance {r.VariancePoints:F2} pts (threshold {VarianceAlertThresholdPts})",
                PeriodStart = from,
                PeriodEnd = to,
                VariancePercent = r.VariancePoints
            });
        }

        await _db.SaveChangesAsync(ct);
        await _audit.AppendAsync(companyId, branchId, userId ?? Guid.Empty, "SnapshotGenerated", "FoodCostSnapshot", snap.Id, ct: ct);
        return snap;
    }
}

public class RecipeProfitabilityService : IRecipeProfitabilityService
{
    private readonly RestBarContext _db;
    private readonly IFoodCostEngine _engine;

    public RecipeProfitabilityService(RestBarContext db, IFoodCostEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    public async Task<PlateCostResult> RecalcAndHistoryAsync(Guid productId, Guid companyId, CancellationToken ct = default)
    {
        var cost = await _engine.GetPlateCostAsync(productId, ct);
        var recipe = await _db.Recipes.AsNoTracking().FirstOrDefaultAsync(r => r.ProductId == productId && r.IsActive, ct);
        if (recipe != null)
            await RecordHistoryAsync(recipe.Id, productId, companyId, cost, "Recalc", ct);
        return cost;
    }

    public async Task RecordHistoryAsync(Guid recipeId, Guid productId, Guid companyId, PlateCostResult cost, string source, CancellationToken ct = default)
    {
        _db.RecipeCostHistories.Add(new RecipeCostHistory
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            ProductId = productId,
            CompanyId = companyId,
            TheoreticalCost = cost.TheoreticalCost,
            FoodCostPercent = cost.FoodCostPercent,
            MarginAmount = cost.GrossMargin,
            Source = source
        });
        await _db.SaveChangesAsync(ct);
    }
}

public class WasteService : IWasteService
{
    private readonly RestBarContext _db;
    private readonly IProductService _products;
    private readonly IInventoryOperationsService _inventory;
    private readonly IFoodCostIntegrityService _audit;

    public WasteService(RestBarContext db, IProductService products, IInventoryOperationsService inventory, IFoodCostIntegrityService audit)
    {
        _db = db;
        _products = products;
        _inventory = inventory;
        _audit = audit;
    }

    public async Task<WasteEvent> RecordWasteAsync(WasteRequest request, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        var unitCost = product.AverageCost ?? product.Cost ?? 0m;
        var before = await _products.GetAvailableStockAsync(request.ProductId, request.BranchId);
        await _products.ReduceStockAsync(request.ProductId, request.Quantity, request.StationId, request.BranchId);
        var after = await _products.GetAvailableStockAsync(request.ProductId, request.BranchId);

        var movement = await _inventory.LogMovementAsync(
            request.ProductId, InventoryMovementType.Waste, -request.Quantity,
            before, after, request.StationId, request.BranchId, request.CompanyId,
            request.ResponsibleUserId, null, request.ReasonCode.ToString(), "WasteEvent");
        movement.UnitCost = unitCost;
        await _db.SaveChangesAsync(ct);

        var evt = new WasteEvent
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            BranchId = request.BranchId,
            ProductId = request.ProductId,
            StationId = request.StationId,
            Quantity = request.Quantity,
            UnitCost = unitCost,
            TotalCost = Math.Round(request.Quantity * unitCost, 2),
            ReasonCode = request.ReasonCode,
            ReasonNotes = request.Notes,
            ResponsibleUserId = request.ResponsibleUserId,
            ApprovedByUserId = request.ApprovedBy,
            InventoryMovementId = movement.Id
        };
        _db.WasteEvents.Add(evt);
        await _db.SaveChangesAsync(ct);
        await _audit.AppendAsync(request.CompanyId, request.BranchId, request.ResponsibleUserId,
            "WasteRecorded", "WasteEvent", evt.Id, $"{{\"cost\":{evt.TotalCost}}}", ct);
        return evt;
    }
}

public class MenuEngineeringService : IMenuEngineeringService
{
    private readonly RestBarContext _db;
    private readonly IFoodCostEngine _engine;

    public MenuEngineeringService(RestBarContext db, IFoodCostEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    public async Task<IReadOnlyList<MenuEngineeringItem>> AnalyzeAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sales = await _db.OrderItems.AsNoTracking()
            .Where(i => i.BranchId == branchId && i.CompanyId == companyId &&
                        i.CreatedAt >= from && i.CreatedAt < to &&
                        i.Status != OrderItemStatus.Cancelled && i.ProductId != null)
            .GroupBy(i => i.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                Qty = g.Sum(x => x.Quantity),
                Sales = g.Sum(x => x.Quantity * x.UnitPrice - x.Discount),
                Theo = g.Sum(x => (x.TheoreticalUnitCost ?? 0) * x.Quantity)
            })
            .ToListAsync(ct);

        if (sales.Count == 0) return Array.Empty<MenuEngineeringItem>();

        var names = await _db.Products.AsNoTracking()
            .Where(p => sales.Select(s => s.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var rows = new List<(Guid Id, string Name, decimal Qty, decimal Sales, decimal Theo, decimal Contrib)>();
        foreach (var s in sales)
        {
            var theo = s.Theo;
            if (theo == 0)
            {
                var plate = await _engine.GetPlateCostAsync(s.ProductId, ct);
                theo = plate.TheoreticalCost * s.Qty;
            }
            rows.Add((s.ProductId, names.GetValueOrDefault(s.ProductId, "?"), s.Qty, s.Sales, theo, s.Sales - theo));
        }

        var popMedian = rows.OrderBy(r => r.Qty).Skip(rows.Count / 2).First().Qty;
        var profitMedian = rows.OrderBy(r => r.Contrib).Skip(rows.Count / 2).First().Contrib;
        if (popMedian == 0) popMedian = 1;
        if (profitMedian == 0) profitMedian = 0.01m;

        return rows.Select(r =>
        {
            var q = MenuEngineeringClassifier.Classify(r.Qty, r.Contrib, popMedian, profitMedian);
            return new MenuEngineeringItem(r.Id, r.Name, r.Qty, r.Sales, r.Theo, r.Contrib, r.Qty, r.Contrib, q, MenuEngineeringClassifier.Recommend(q));
        }).OrderByDescending(x => x.Contribution).ToList();
    }
}

public class CostSimulationService : ICostSimulationService
{
    public CostSimulationResult Simulate(CostSimulationRequest request)
    {
        var price = request.NewPrice ?? request.CurrentPrice;
        var plate = request.NewPlateCost ?? request.CurrentPlateCost;

        if (request.IngredientCostDeltaPercent.HasValue)
            plate = Math.Round(plate * (1 + request.IngredientCostDeltaPercent.Value / 100m), 4);
        if (request.RecipeQtyDeltaPercent.HasValue)
            plate = Math.Round(plate * (1 + request.RecipeQtyDeltaPercent.Value / 100m), 4);

        var fc = FoodCostMath.FoodCostPercent(plate, price);
        var margin = FoodCostMath.GrossMargin(price, plate);
        var marginPct = FoodCostMath.GrossMarginPercent(price, plate);
        var delta = margin - FoodCostMath.GrossMargin(request.CurrentPrice, request.CurrentPlateCost);
        return new CostSimulationResult(price, plate, fc, margin, marginPct, delta);
    }
}

public class FoodCostDashboardService : IFoodCostDashboardService
{
    private readonly IFoodCostEngine _engine;
    private readonly IMenuEngineeringService _menu;
    private readonly RestBarContext _db;

    public FoodCostDashboardService(IFoodCostEngine engine, IMenuEngineeringService menu, RestBarContext db)
    {
        _engine = engine;
        _menu = menu;
        _db = db;
    }

    public async Task<object> GetCommandCenterAsync(Guid companyId, Guid branchId, CancellationToken ct = default)
    {
        var to = DateTime.UtcNow;
        var from = to.Date;
        var period = await _engine.GetPeriodAnalysisAsync(companyId, branchId, from, to, ct);
        var monthFrom = to.AddDays(-30);
        var menu = await _menu.AnalyzeAsync(companyId, branchId, monthFrom, to, ct);
        var wasteToday = await _db.WasteEvents.AsNoTracking()
            .Where(w => w.BranchId == branchId && w.CreatedAt >= from)
            .SumAsync(w => (decimal?)w.TotalCost, ct) ?? 0m;
        var alerts = await _db.VarianceAlerts.AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsResolved)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToListAsync(ct);

        return new
        {
            Today = period,
            WasteToday = wasteToday,
            Stars = menu.Where(m => m.Quadrant == MenuQuadrant.Star).Take(5),
            Dogs = menu.Where(m => m.Quadrant == MenuQuadrant.Dog).Take(5),
            TopContribution = menu.Take(5),
            Alerts = alerts
        };
    }
}

public class OrderItemCostHook : IOrderItemCostHook
{
    private readonly IFoodCostEngine _engine;
    private readonly FeatureFlags _flags;
    private readonly RestBarContext _db;

    public OrderItemCostHook(IFoodCostEngine engine, IOptions<FeatureFlags> flags, RestBarContext db)
    {
        _engine = engine;
        _flags = flags.Value;
        _db = db;
    }

    public async Task ApplyAsync(OrderItem item, CancellationToken ct = default)
    {
        if (!_flags.EnableFoodCostModule || item.ProductId == null)
            return;
        if (item.TheoreticalUnitCost.HasValue)
            return;

        try
        {
            var plate = await _engine.GetPlateCostAsync(item.ProductId.Value, ct);
            item.TheoreticalUnitCost = plate.TheoreticalCost;
            item.CostSnapshotAt = DateTime.UtcNow;
            _db.OrderItems.Update(item);
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Never break POS
        }
    }
}
