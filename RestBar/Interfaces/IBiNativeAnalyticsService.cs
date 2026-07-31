namespace RestBar.Interfaces;

public interface IBiNativeAnalyticsService
{
    Task<BiSalesSummaryRow?> GetSalesSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiHourlySalesRow>> GetHourlySalesAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiTopProductRow>> GetTopProductsAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<BiWaiterRow>> GetWaiterPerformanceAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiStationRow>> GetStationPerformanceAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<BiCashSummaryRow?> GetCashSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<BiInventoryHealthRow?> GetInventoryHealthAsync(Guid companyId, Guid branchId, CancellationToken ct = default);
    Task<BiFoodCostSummaryRow?> GetFoodCostSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiTopWasteRow>> GetTopWasteAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, int limit = 20, CancellationToken ct = default);
    Task<BiPurchaseAnalysisRow?> GetPurchaseAnalysisAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiSupplierRow>> GetSupplierAnalysisAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<BiProfitabilityRow?> GetProfitabilityAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IReadOnlyList<BiBranchComparisonRow>> GetBranchComparisonAsync(Guid companyId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<BiExecutiveDashboardRow?> GetExecutiveDashboardAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
}

public record BiSalesSummaryRow(long OrderCount, decimal Revenue, decimal AvgTicket, long CancelledCount, decimal DiscountTotal, long CompletedCount);
public record BiHourlySalesRow(int SaleHour, long OrderCount, decimal Revenue);
public record BiTopProductRow(Guid ProductId, string ProductName, decimal QtySold, decimal Revenue, decimal CogsEstimate, decimal MarginEstimate);
public record BiWaiterRow(Guid UserId, string WaiterName, long OrderCount, decimal Revenue, decimal AvgTicket);
public record BiStationRow(Guid StationId, string StationName, long ItemsProcessed, long OrdersProcessed, decimal AvgPrepMinutes, decimal Revenue);
public record BiCashSummaryRow(long SessionsOpened, long SessionsClosed, decimal TotalSales, decimal TotalRefunds, decimal TotalPaidIn, decimal TotalPaidOut, decimal TotalVariance, decimal AbsVariance);
public record BiInventoryHealthRow(long TrackedProducts, long LowStockCount, long ZeroStockCount, decimal StockValueEstimate, decimal WasteQty30d, decimal WasteCost30d, long SaleMovements30d);
public record BiFoodCostSummaryRow(decimal SalesTotal, decimal TheoreticalCogs, decimal ActualCogs, decimal WasteCost, decimal FoodCostPctTheo, decimal FoodCostPctActual, long SnapshotCount);
public record BiTopWasteRow(Guid ProductId, string ProductName, long Events, decimal Qty, decimal TotalCost);
public record BiPurchaseAnalysisRow(long PoCount, decimal PoTotal, long ReceiptCount, long OpenPoCount, long OverduePoCount, decimal AvgLeadDays);
public record BiSupplierRow(Guid SupplierId, string SupplierName, long PoCount, decimal PoTotal, long ReceiptCount);
public record BiProfitabilityRow(decimal Revenue, decimal CogsEstimate, decimal GrossProfit, decimal GrossMarginPct, long ItemCount);
public record BiBranchComparisonRow(Guid BranchId, string BranchName, long OrderCount, decimal Revenue, decimal AvgTicket);
public record BiExecutiveDashboardRow(decimal Revenue, long OrdersCompleted, decimal AvgTicket, decimal GrossMarginPct, decimal CashVariance, long LowStockCount, decimal WasteCost, long OpenPoCount);
