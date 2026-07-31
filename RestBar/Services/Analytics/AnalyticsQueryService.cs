using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Analytics;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Analytics;

public sealed class AnalyticsQueryService : IAnalyticsQueryService
{
    private readonly RestBarContext _db;
    private readonly IBiNativeAnalyticsService _bi;

    public AnalyticsQueryService(RestBarContext db, IBiNativeAnalyticsService bi)
    {
        _db = db;
        _bi = bi;
    }

    public async Task<object?> GetReportDataAsync(string reportKey, AnalyticsFilter f, CancellationToken ct = default)
    {
        var def = AnalyticsReportCatalog.Get(reportKey)
            ?? throw new KeyNotFoundException($"Unknown report '{reportKey}'");
        if (def.Availability is KpiAvailability.NotAvailable or KpiAvailability.RequiresModelChange)
            return new { available = false, limitation = def.Limitation, definition = def };

        return reportKey.ToLowerInvariant() switch
        {
            "executive-summary" => await ExecSummary(f, ct),
            "branch-comparison" => await Many("SELECT * FROM analytics.sp_sales_by_branch({0},{1},{2})", ct, f.CompanyId, f.StartUtc, f.EndUtc),
            "period-results" => await Period(f, ct),
            "sales-trend" => await Many("SELECT * FROM analytics.sp_sales_trend({0},{1},{2},{3},{4})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, "day"),
            "sales-hour" => await Many("SELECT * FROM analytics.sp_sales_by_hour({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "sales-product" or "profitability-product" => await Many("SELECT * FROM analytics.sp_sales_by_product({0},{1},{2},{3},{4})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, 100),
            "sales-category" => await Many("SELECT * FROM analytics.sp_sales_by_category({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "sales-waiter" or "waiters" => await Many("SELECT * FROM analytics.sp_waiter_performance({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "payment-methods" => await Many("SELECT * FROM analytics.sp_sales_by_payment({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "food-cost" => await One("SELECT * FROM analytics.sp_food_cost_summary({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "menu-engineering" => await Many("SELECT * FROM analytics.sp_menu_engineering({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "waste" => await Many("SELECT * FROM analytics.sp_waste_analysis({0},{1},{2},{3},{4})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, 50),
            "inventory-health" => await One("SELECT * FROM analytics.sp_inventory_health({0},{1})", ct, f.CompanyId, f.BranchId),
            "inventory-turnover" => await Many("SELECT * FROM analytics.sp_inventory_turnover({0},{1},{2})", ct, f.CompanyId, f.BranchId, 30),
            "inventory-coverage" => await Many("SELECT * FROM analytics.sp_inventory_coverage({0},{1},{2})", ct, f.CompanyId, f.BranchId, 30),
            "purchases-supplier" => await Many("SELECT * FROM analytics.sp_supplier_performance({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "purchase-summary" => await One("SELECT * FROM analytics.sp_purchase_summary({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "price-variation" => await Many("SELECT * FROM analytics.sp_supplier_price_variation({0},{1},{2})", ct, f.CompanyId, f.StartUtc, f.EndUtc),
            "cash-summary" => await One("SELECT * FROM analytics.sp_cash_summary({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "cash-variance" => await Many("SELECT * FROM analytics.sp_cash_variance({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "kitchen" => await One("SELECT * FROM analytics.sp_kitchen_performance({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "stations" => await Many("SELECT * FROM analytics.sp_station_performance({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            "table-turnover" => await Many("SELECT * FROM analytics.sp_table_turnover({0},{1},{2},{3})", ct, f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc),
            _ => throw new KeyNotFoundException(reportKey)
        };
    }

    public async Task<AnalyticsLiveSnapshot> GetLiveAsync(AnalyticsFilter f, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var salesToday = await _db.Orders.AsNoTracking()
            .Where(o => o.CompanyId == f.CompanyId && o.BranchId == f.BranchId
                        && o.Status == OrderStatus.Completed && o.ClosedAt >= today && o.ClosedAt < tomorrow)
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0m, ct);

        var openOrders = await _db.Orders.AsNoTracking()
            .CountAsync(o => o.CompanyId == f.CompanyId && o.BranchId == f.BranchId
                             && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled, ct);

        var cutoff = DateTime.UtcNow.AddMinutes(-20);
        var delayed = await (
            from oi in _db.OrderItems.AsNoTracking()
            join o in _db.Orders.AsNoTracking() on oi.OrderId equals o.Id
            where o.CompanyId == f.CompanyId && o.BranchId == f.BranchId
                  && oi.SentAt != null && oi.PreparedAt == null && oi.SentAt < cutoff
                  && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled
            select oi.Id).CountAsync(ct);

        var occupied = await _db.Tables.AsNoTracking()
            .CountAsync(t => t.CompanyId == f.CompanyId && t.BranchId == f.BranchId && t.IsActive && t.Status == TableStatus.Ocupada, ct);
        var free = await _db.Tables.AsNoTracking()
            .CountAsync(t => t.CompanyId == f.CompanyId && t.BranchId == f.BranchId && t.IsActive && t.Status == TableStatus.Disponible, ct);

        var openCash = await _db.CashSessions.AsNoTracking()
            .CountAsync(s => s.CompanyId == f.CompanyId && s.BranchId == f.BranchId
                             && (s.Status == CashSessionStatus.Open || s.Status == CashSessionStatus.Operating), ct);

        var cashIncidents = await _db.CashSessions.AsNoTracking()
            .CountAsync(s => s.CompanyId == f.CompanyId && s.BranchId == f.BranchId
                             && s.OpenedAt >= today && s.Variance != 0, ct);

        var critical = await _db.Products.AsNoTracking()
            .CountAsync(p => p.CompanyId == f.CompanyId && p.TrackInventory && p.IsActive
                             && p.MinStock != null && p.Stock <= p.MinStock, ct);
        var zero = await _db.Products.AsNoTracking()
            .CountAsync(p => p.CompanyId == f.CompanyId && p.TrackInventory && p.IsActive && p.Stock <= 0, ct);

        var overduePo = await _db.PurchaseOrders.AsNoTracking()
            .CountAsync(po => po.CompanyId == f.CompanyId && po.BranchId == f.BranchId
                              && po.ExpectedDelivery != null && po.ExpectedDelivery < DateTime.UtcNow
                              && po.Status != PurchaseOrderStatus.Closed && po.Status != PurchaseOrderStatus.Cancelled
                              && po.Status != PurchaseOrderStatus.FullyReceived, ct);

        var wasteToday = await _db.WasteEvents.AsNoTracking()
            .CountAsync(w => w.CompanyId == f.CompanyId && w.BranchId == f.BranchId
                             && w.CreatedAt >= today && w.CreatedAt < tomorrow, ct);

        return new AnalyticsLiveSnapshot(salesToday, openOrders, delayed, occupied, free, openCash, cashIncidents,
            critical, zero, overduePo, wasteToday, DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<AnalyticsDecision>> GetDecisionsAsync(AnalyticsFilter f, CancellationToken ct = default)
    {
        var list = new List<AnalyticsDecision>();
        var period = $"{f.StartUtc:yyyy-MM-dd} → {f.EndUtc:yyyy-MM-dd}";

        var products = await _bi.GetTopProductsAsync(f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, 50, ct);
        foreach (var p in products.Where(x => x.MarginEstimate < 0).Take(5))
        {
            list.Add(new AnalyticsDecision("NEG_MARGIN", $"Producto con margen estimado negativo: {p.ProductName}",
                "SAL.NEG_MARGIN", p.MarginEstimate, 0, $"Ingreso {p.Revenue:0.00}", "High", f.BranchId, period,
                "Revisar costo/receta o precio de venta", "profitability-product", true));
        }

        var cash = await _bi.GetCashSummaryAsync(f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ct);
        if (cash != null && Math.Abs(cash.TotalVariance) > 0)
        {
            list.Add(new AnalyticsDecision("CASH_VAR", "Hay diferencia acumulada de caja en el periodo",
                "EXE.CASH_VAR", cash.TotalVariance, 0, $"Abs {cash.AbsVariance:0.00}", "High", f.BranchId, period,
                "Abrir diferencias de caja y auditar sesiones", "cash-variance", false));
        }

        var inv = await _bi.GetInventoryHealthAsync(f.CompanyId, f.BranchId, ct);
        if (inv != null && inv.LowStockCount > 0)
        {
            list.Add(new AnalyticsDecision("LOW_STOCK", "Productos en stock crítico",
                "INV.STOCK", inv.LowStockCount, 0, $"{inv.ZeroStockCount} en cero", "Medium", f.BranchId, period,
                "Revisar cobertura y generar compra", "inventory-coverage", false));
        }

        var purch = await _bi.GetPurchaseAnalysisAsync(f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ct);
        if (purch != null && purch.OverduePoCount > 0)
        {
            list.Add(new AnalyticsDecision("OVERDUE_PO", "Órdenes de compra atrasadas",
                "EXE.PURCHASES", purch.OverduePoCount, 0, null, "High", f.BranchId, period,
                "Seguir proveedores atrasados", "purchase-summary", false));
        }

        var (ps, pe) = f.CompareStartUtc.HasValue
            ? (f.CompareStartUtc.Value, f.CompareEndUtc!.Value)
            : AnalyticsPeriodHelper.PreviousEqualLength(f.StartUtc, f.EndUtc);
        var comp = await Many("SELECT * FROM analytics.sp_period_comparison({0},{1},{2},{3},{4},{5})", ct,
            f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ps, pe);
        var rev = comp.FirstOrDefault(r => string.Equals(Convert.ToString(r.GetValueOrDefault("metric")), "revenue", StringComparison.OrdinalIgnoreCase));
        if (rev != null && rev.TryGetValue("pct_change", out var pctObj) && pctObj != null
            && decimal.TryParse(pctObj.ToString(), out var pct) && pct <= -15)
        {
            list.Add(new AnalyticsDecision("SALES_DROP", "Caída de ventas vs periodo comparable ≥ 15%",
                "EXE.PERIOD_COMP", pct, -15, null, "High", f.BranchId, period,
                "Drill-down por hora/categoría/mesero", "sales-trend", true));
        }

        var kitchenRows = await Many("SELECT * FROM analytics.sp_kitchen_performance({0},{1},{2},{3})", ct,
            f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc);
        var kitchen = kitchenRows.FirstOrDefault();
        if (kitchen != null && kitchen.TryGetValue("delayed_items_gt_20m", out var d) && d != null && Convert.ToInt64(d) > 0)
        {
            list.Add(new AnalyticsDecision("KITCHEN_DELAY", "Ítems con prep > 20 min",
                "EXE.PREP_TIME", Convert.ToDecimal(d), 0, null, "Medium", f.BranchId, period,
                "Revisar estaciones atrasadas", "stations", true));
        }

        return list.OrderBy(x => x.Priority == "High" ? 0 : x.Priority == "Medium" ? 1 : 2).ToList();
    }

    private async Task<object> ExecSummary(AnalyticsFilter f, CancellationToken ct)
    {
        var exec = await _bi.GetExecutiveDashboardAsync(f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ct);
        var (ps, pe) = f.CompareStartUtc.HasValue
            ? (f.CompareStartUtc.Value, f.CompareEndUtc!.Value)
            : AnalyticsPeriodHelper.PreviousEqualLength(f.StartUtc, f.EndUtc);
        var comparison = await Many("SELECT * FROM analytics.sp_period_comparison({0},{1},{2},{3},{4},{5})", ct,
            f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ps, pe);
        return new { available = true, executive = exec, comparison, filter = Meta(f) };
    }

    private async Task<object> Period(AnalyticsFilter f, CancellationToken ct)
    {
        var (ps, pe) = f.CompareStartUtc.HasValue
            ? (f.CompareStartUtc.Value, f.CompareEndUtc!.Value)
            : AnalyticsPeriodHelper.PreviousEqualLength(f.StartUtc, f.EndUtc);
        var rows = await Many("SELECT * FROM analytics.sp_period_comparison({0},{1},{2},{3},{4},{5})", ct,
            f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, ps, pe);
        return new { available = true, rows, filter = Meta(f) };
    }

    private static object Meta(AnalyticsFilter f) => new
    {
        f.CompanyId, f.BranchId, f.StartUtc, f.EndUtc, f.CompareStartUtc, f.CompareEndUtc, f.Currency, f.TimeZone
    };

    private async Task<List<Dictionary<string, object?>>> Many(string sql, CancellationToken ct, params object[] args)
    {
        await using var conn = _db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = Rewrite(sql, args.Length);
        for (var i = 0; i < args.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = $"@p{i}";
            p.Value = args[i] ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        var list = new List<Dictionary<string, object?>>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            list.Add(row);
        }
        return list;
    }

    private async Task<object?> One(string sql, CancellationToken ct, params object[] args)
        => (await Many(sql, ct, args)).FirstOrDefault();

    private static string Rewrite(string sql, int n)
    {
        for (var i = 0; i < n; i++)
            sql = sql.Replace($"{{{i}}}", $"@p{i}");
        return sql;
    }
}
