using Microsoft.EntityFrameworkCore;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Intelligence;

public class BiNativeAnalyticsService : IBiNativeAnalyticsService
{
    private readonly RestBarContext _db;

    public BiNativeAnalyticsService(RestBarContext db) => _db = db;

    public async Task<BiSalesSummaryRow?> GetSalesSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<SalesSummarySql>("SELECT * FROM sp_sales_summary({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiSalesSummaryRow(row.order_count, row.revenue, row.avg_ticket, row.cancelled_count, row.discount_total, row.completed_count);
    }

    public async Task<IReadOnlyList<BiHourlySalesRow>> GetHourlySalesAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var rows = await ManyAsync<HourlySalesSql>("SELECT * FROM sp_hourly_sales({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return rows.Select(r => new BiHourlySalesRow(r.sale_hour, r.order_count, r.revenue)).ToList();
    }

    public async Task<IReadOnlyList<BiTopProductRow>> GetTopProductsAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, int limit = 20, CancellationToken ct = default)
    {
        var rows = await ManyAsync<TopProductSql>("SELECT * FROM sp_top_products({0}, {1}, {2}, {3}, {4})", ct, companyId, branchId, startUtc, endUtc, limit);
        return rows.Select(r => new BiTopProductRow(r.product_id, r.product_name, r.qty_sold, r.revenue, r.cogs_estimate, r.margin_estimate)).ToList();
    }

    public async Task<IReadOnlyList<BiWaiterRow>> GetWaiterPerformanceAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var rows = await ManyAsync<WaiterSql>("SELECT * FROM sp_waiter_performance({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return rows.Select(r => new BiWaiterRow(r.user_id, r.waiter_name, r.order_count, r.revenue, r.avg_ticket)).ToList();
    }

    public async Task<IReadOnlyList<BiStationRow>> GetStationPerformanceAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var rows = await ManyAsync<StationSql>("SELECT * FROM sp_station_performance({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return rows.Select(r => new BiStationRow(r.station_id, r.station_name, r.items_processed, r.orders_processed, r.avg_prep_minutes, r.revenue)).ToList();
    }

    public async Task<BiCashSummaryRow?> GetCashSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<CashSummarySql>("SELECT * FROM sp_cash_summary({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiCashSummaryRow(row.sessions_opened, row.sessions_closed, row.total_sales, row.total_refunds, row.total_paid_in, row.total_paid_out, row.total_variance, row.abs_variance);
    }

    public async Task<BiInventoryHealthRow?> GetInventoryHealthAsync(Guid companyId, Guid branchId, CancellationToken ct = default)
    {
        var row = await OneAsync<InventoryHealthSql>("SELECT * FROM sp_inventory_health({0}, {1})", ct, companyId, branchId);
        return row is null ? null : new BiInventoryHealthRow(row.tracked_products, row.low_stock_count, row.zero_stock_count, row.stock_value_estimate, row.waste_qty_30d, row.waste_cost_30d, row.sale_movements_30d);
    }

    public async Task<BiFoodCostSummaryRow?> GetFoodCostSummaryAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<FoodCostSql>("SELECT * FROM sp_food_cost_summary({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiFoodCostSummaryRow(row.sales_total, row.theoretical_cogs, row.actual_cogs, row.waste_cost, row.food_cost_pct_theo, row.food_cost_pct_actual, row.snapshot_count);
    }

    public async Task<IReadOnlyList<BiTopWasteRow>> GetTopWasteAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, int limit = 20, CancellationToken ct = default)
    {
        var rows = await ManyAsync<TopWasteSql>("SELECT * FROM sp_top_waste({0}, {1}, {2}, {3}, {4})", ct, companyId, branchId, startUtc, endUtc, limit);
        return rows.Select(r => new BiTopWasteRow(r.product_id, r.product_name, r.events, r.qty, r.total_cost)).ToList();
    }

    public async Task<BiPurchaseAnalysisRow?> GetPurchaseAnalysisAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<PurchaseSql>("SELECT * FROM sp_purchase_analysis({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiPurchaseAnalysisRow(row.po_count, row.po_total, row.receipt_count, row.open_po_count, row.overdue_po_count, row.avg_lead_days);
    }

    public async Task<IReadOnlyList<BiSupplierRow>> GetSupplierAnalysisAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var rows = await ManyAsync<SupplierSql>("SELECT * FROM sp_supplier_analysis({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return rows.Select(r => new BiSupplierRow(r.supplier_id, r.supplier_name, r.po_count, r.po_total, r.receipt_count)).ToList();
    }

    public async Task<BiProfitabilityRow?> GetProfitabilityAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<ProfitSql>("SELECT * FROM sp_profitability({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiProfitabilityRow(row.revenue, row.cogs_estimate, row.gross_profit, row.gross_margin_pct, row.item_count);
    }

    public async Task<IReadOnlyList<BiBranchComparisonRow>> GetBranchComparisonAsync(Guid companyId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var rows = await ManyAsync<BranchSql>("SELECT * FROM sp_branch_comparison({0}, {1}, {2})", ct, companyId, startUtc, endUtc);
        return rows.Select(r => new BiBranchComparisonRow(r.branch_id, r.branch_name, r.order_count, r.revenue, r.avg_ticket)).ToList();
    }

    public async Task<BiExecutiveDashboardRow?> GetExecutiveDashboardAsync(Guid companyId, Guid branchId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
    {
        var row = await OneAsync<ExecSql>("SELECT * FROM sp_executive_dashboard({0}, {1}, {2}, {3})", ct, companyId, branchId, startUtc, endUtc);
        return row is null ? null : new BiExecutiveDashboardRow(row.revenue, row.orders_completed, row.avg_ticket, row.gross_margin_pct, row.cash_variance, row.low_stock_count, row.waste_cost, row.open_po_count);
    }

    private async Task<T?> OneAsync<T>(string sql, CancellationToken ct, params object[] args) where T : class
        => (await ManyAsync<T>(sql, ct, args)).FirstOrDefault();

    private Task<List<T>> ManyAsync<T>(string sql, CancellationToken ct, params object[] args) where T : class
        => _db.Database.SqlQueryRaw<T>(sql, args).ToListAsync(ct);

    private sealed class SalesSummarySql { public long order_count { get; set; } public decimal revenue { get; set; } public decimal avg_ticket { get; set; } public long cancelled_count { get; set; } public decimal discount_total { get; set; } public long completed_count { get; set; } }
    private sealed class HourlySalesSql { public int sale_hour { get; set; } public long order_count { get; set; } public decimal revenue { get; set; } }
    private sealed class TopProductSql { public Guid product_id { get; set; } public string product_name { get; set; } = ""; public decimal qty_sold { get; set; } public decimal revenue { get; set; } public decimal cogs_estimate { get; set; } public decimal margin_estimate { get; set; } }
    private sealed class WaiterSql { public Guid user_id { get; set; } public string waiter_name { get; set; } = ""; public long order_count { get; set; } public decimal revenue { get; set; } public decimal avg_ticket { get; set; } }
    private sealed class StationSql { public Guid station_id { get; set; } public string station_name { get; set; } = ""; public long items_processed { get; set; } public long orders_processed { get; set; } public decimal avg_prep_minutes { get; set; } public decimal revenue { get; set; } }
    private sealed class CashSummarySql { public long sessions_opened { get; set; } public long sessions_closed { get; set; } public decimal total_sales { get; set; } public decimal total_refunds { get; set; } public decimal total_paid_in { get; set; } public decimal total_paid_out { get; set; } public decimal total_variance { get; set; } public decimal abs_variance { get; set; } }
    private sealed class InventoryHealthSql { public long tracked_products { get; set; } public long low_stock_count { get; set; } public long zero_stock_count { get; set; } public decimal stock_value_estimate { get; set; } public decimal waste_qty_30d { get; set; } public decimal waste_cost_30d { get; set; } public long sale_movements_30d { get; set; } }
    private sealed class FoodCostSql { public decimal sales_total { get; set; } public decimal theoretical_cogs { get; set; } public decimal actual_cogs { get; set; } public decimal waste_cost { get; set; } public decimal food_cost_pct_theo { get; set; } public decimal food_cost_pct_actual { get; set; } public long snapshot_count { get; set; } }
    private sealed class TopWasteSql { public Guid product_id { get; set; } public string product_name { get; set; } = ""; public long events { get; set; } public decimal qty { get; set; } public decimal total_cost { get; set; } }
    private sealed class PurchaseSql { public long po_count { get; set; } public decimal po_total { get; set; } public long receipt_count { get; set; } public long open_po_count { get; set; } public long overdue_po_count { get; set; } public decimal avg_lead_days { get; set; } }
    private sealed class SupplierSql { public Guid supplier_id { get; set; } public string supplier_name { get; set; } = ""; public long po_count { get; set; } public decimal po_total { get; set; } public long receipt_count { get; set; } }
    private sealed class ProfitSql { public decimal revenue { get; set; } public decimal cogs_estimate { get; set; } public decimal gross_profit { get; set; } public decimal gross_margin_pct { get; set; } public long item_count { get; set; } }
    private sealed class BranchSql { public Guid branch_id { get; set; } public string branch_name { get; set; } = ""; public long order_count { get; set; } public decimal revenue { get; set; } public decimal avg_ticket { get; set; } }
    private sealed class ExecSql { public decimal revenue { get; set; } public long orders_completed { get; set; } public decimal avg_ticket { get; set; } public decimal gross_margin_pct { get; set; } public decimal cash_variance { get; set; } public long low_stock_count { get; set; } public decimal waste_cost { get; set; } public long open_po_count { get; set; } }
}
