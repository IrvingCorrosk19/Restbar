using RestBar.Models;

namespace RestBar.Interfaces;

public interface IExecutiveCommandCenterService
{
    Task<ExecutiveCommandCenterDto> GetSnapshotAsync(Guid companyId, Guid branchId, Guid? actorUserId = null, CancellationToken ct = default);
}

public interface IBiInsightEngine
{
    IReadOnlyList<BiInsightDraft> Generate(ExecutiveSignals signals);
}

public interface IBiAlertEngine
{
    IReadOnlyList<BiAlertDraft> Evaluate(ExecutiveSignals signals);
}

public interface IBiScoreEngine
{
    BiScoreResult Compute(ExecutiveSignals signals);
}

public record ExecutiveSignals(
    decimal RevenueToday,
    decimal RevenueYesterday,
    int OrdersToday,
    decimal AverageTicket,
    decimal GrossMarginPct,
    decimal TheoFoodCostPct,
    decimal ActualFoodCostPct,
    decimal VariancePts,
    decimal WasteToday,
    int OpenPurchaseOrders,
    int OverdueOrders,
    int CriticalSuppliers,
    int LowStockCount,
    decimal ExpectedCash,
    int ActiveCashSessions);

public record BiInsightDraft(BiInsightType Type, BiSeverity Severity, string Title, string Explanation, string Action);
public record BiAlertDraft(string Code, BiSeverity Severity, string Message, string SourceModule);
public record BiScoreResult(decimal Enterprise, decimal Financial, decimal Operational, decimal FoodCost, decimal Procurement);

public record ExecutiveCommandCenterDto(
    decimal EnterpriseScore,
    BiScoreResult Scores,
    decimal RevenueToday,
    decimal RevenueYesterday,
    decimal SalesDropPercent,
    int OrdersToday,
    decimal AverageTicket,
    decimal ExpectedCash,
    int ActiveCashSessions,
    decimal TheoFoodCostPct,
    decimal ActualFoodCostPct,
    decimal VariancePts,
    decimal WasteToday,
    int OpenPurchaseOrders,
    int OverdueOrders,
    int CriticalSuppliers,
    int LowStockCount,
    IReadOnlyList<BiInsightDraft> Insights,
    IReadOnlyList<BiAlertDraft> Alerts,
    IReadOnlyList<string> TopActions,
    DateTime GeneratedAtUtc,
    int DurationMs);
