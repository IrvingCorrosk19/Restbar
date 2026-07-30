using RestBar.Models;

namespace RestBar.Interfaces;

public interface IFoodCostEngine
{
    Task<PlateCostResult> GetPlateCostAsync(Guid productId, CancellationToken ct = default);
    Task<FoodCostPeriodResult> GetPeriodAnalysisAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<FoodCostSnapshot> GenerateSnapshotAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, Guid? userId, CancellationToken ct = default);
}

public interface IRecipeProfitabilityService
{
    Task<PlateCostResult> RecalcAndHistoryAsync(Guid productId, Guid companyId, CancellationToken ct = default);
    Task RecordHistoryAsync(Guid recipeId, Guid productId, Guid companyId, PlateCostResult cost, string source, CancellationToken ct = default);
}

public interface IWasteService
{
    Task<WasteEvent> RecordWasteAsync(WasteRequest request, CancellationToken ct = default);
}

public interface IMenuEngineeringService
{
    Task<IReadOnlyList<MenuEngineeringItem>> AnalyzeAsync(Guid companyId, Guid branchId, DateTime from, DateTime to, CancellationToken ct = default);
}

public interface ICostSimulationService
{
    CostSimulationResult Simulate(CostSimulationRequest request);
}

public interface IFoodCostDashboardService
{
    Task<object> GetCommandCenterAsync(Guid companyId, Guid branchId, CancellationToken ct = default);
}

public interface IFoodCostIntegrityService
{
    Task AppendAsync(Guid companyId, Guid branchId, Guid actorId, string eventType, string entityType, Guid? entityId, string? afterJson = null, CancellationToken ct = default);
}

public interface IOrderItemCostHook
{
    Task ApplyAsync(OrderItem item, CancellationToken ct = default);
}

public record PlateCostResult(
    Guid ProductId,
    decimal TheoreticalCost,
    decimal SellingPrice,
    decimal FoodCostPercent,
    decimal GrossMargin,
    decimal GrossMarginPercent,
    IReadOnlyList<IngredientCostLine> Lines);

public record IngredientCostLine(Guid IngredientProductId, string Name, decimal Qty, decimal WastePercent, decimal UnitCost, decimal LineCost);

public record FoodCostPeriodResult(
    decimal SalesTotal,
    decimal TheoreticalCogs,
    decimal ActualCogs,
    decimal WasteCost,
    decimal VarianceAmount,
    decimal VariancePoints,
    decimal TheoFoodCostPercent,
    decimal ActualFoodCostPercent,
    decimal GrossMarginPercent);

public record WasteRequest(
    Guid CompanyId, Guid BranchId, Guid ProductId, decimal Quantity,
    Guid ResponsibleUserId, WasteReasonCode ReasonCode,
    Guid? StationId = null, string? Notes = null, Guid? ApprovedBy = null);

public record MenuEngineeringItem(
    Guid ProductId, string Name, decimal QtySold, decimal Sales,
    decimal TheoCost, decimal Contribution, decimal PopularityIndex,
    decimal ProfitabilityIndex, MenuQuadrant Quadrant, string Recommendation);

public record CostSimulationRequest(
    decimal CurrentPrice, decimal CurrentPlateCost,
    decimal? NewPrice = null, decimal? NewPlateCost = null,
    decimal? IngredientCostDeltaPercent = null, decimal? RecipeQtyDeltaPercent = null);

public record CostSimulationResult(
    decimal NewPrice, decimal NewPlateCost, decimal NewFoodCostPercent,
    decimal NewMargin, decimal NewMarginPercent, decimal DeltaMargin);
