using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Analytics;
using RestBar.Domain.DecisionIntelligence;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.DecisionIntelligence;

public sealed class DecisionIntelligenceService : IDecisionIntelligenceService
{
    private readonly RestBarContext _db;
    private readonly IAnalyticsQueryService _analytics;
    private readonly IBiNativeAnalyticsService _bi;

    public DecisionIntelligenceService(RestBarContext db, IAnalyticsQueryService analytics, IBiNativeAnalyticsService bi)
    {
        _db = db;
        _analytics = analytics;
        _bi = bi;
    }

    public DiDataQualityBanner GetDataQualityBanner()
    {
        // Design score from RB-028 02_DATA_QUALITY_REPORT (global ~68)
        const int score = 68;
        return new DiDataQualityBanner(score, "Advertencia",
            "Data Quality global estimado 68/100. Forecasts de Food Cost / Labor Cost no se presentan como alta confianza. Ver DECISION_INTELLIGENCE/02_DATA_QUALITY_REPORT.md");
    }

    public async Task<DiCockpitDto> GetCockpitAsync(AnalyticsFilter filter, Guid userId, CancellationToken ct = default)
    {
        var quality = GetDataQualityBanner();
        object? exec = null;
        try { exec = await _analytics.GetReportDataAsync("executive-summary", filter, ct); }
        catch { /* soft */ }

        AnalyticsLiveSnapshot? live = null;
        try { live = await _analytics.GetLiveAsync(filter, ct); }
        catch { /* soft — never fail cockpit */ }

        DiForecastDto? forecast = null;
        try { forecast = await GetSalesForecastAsync(filter, 7, userId, persistRun: true, ct); }
        catch
        {
            try { forecast = await GetSalesForecastAsync(filter, 7, userId, persistRun: false, ct); }
            catch { /* soft */ }
        }

        IReadOnlyList<DiRecommendationDto> recs = Array.Empty<DiRecommendationDto>();
        try { recs = await GetRecommendationsAsync(filter, ct); }
        catch { /* soft */ }

        IReadOnlyList<object> alerts = Array.Empty<object>();
        try
        {
            var branchId = filter.BranchId;
            var cross = filter.CrossBranch || branchId == Guid.Empty;
            var raw = await _db.BiAlerts.AsNoTracking()
                .Where(a => a.CompanyId == filter.CompanyId && !a.IsResolved
                            && (cross || a.BranchId == null || a.BranchId == branchId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .Select(a => new { a.AlertCode, a.Severity, a.Message, a.CreatedAt })
                .ToListAsync(ct);
            alerts = raw.Cast<object>().ToList();
        }
        catch { /* module may be empty */ }

        return new DiCockpitDto(
            quality,
            exec,
            live is null ? [] : [live],
            forecast ?? new DiForecastDto(
                "SALES_DAILY", ForecastEngine.Naive, 7, 0, [], [],
                ForecastAccuracyMetrics.Empty, ForecastAccuracyMetrics.Empty, false, "Baja",
                "Forecast no disponible en este momento.", DateTime.UtcNow),
            recs,
            alerts,
            DateTime.UtcNow.ToString("o"));
    }

    public async Task<DiForecastDto> GetSalesForecastAsync(AnalyticsFilter filter, int horizonDays, Guid? userId, bool persistRun, CancellationToken ct = default)
    {
        horizonDays = Math.Clamp(horizonDays, 1, 90);
        var history = await LoadDailySalesAsync(filter, ct);
        var limitations = new List<string>();
        if (history.Count < 14) limitations.Add("Histórico < 14 días: confianza baja.");
        if (await _db.DiManualEvents.AsNoTracking().AnyAsync(e => e.CompanyId == filter.CompanyId
                && e.EventDate >= DateOnly.FromDateTime(DateTime.UtcNow.Date)
                && e.EventDate <= DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(horizonDays)), ct))
            limitations.Add("Hay eventos manuales en el horizonte: revisar calendario DI.");

        limitations.Add("Modelos estadísticos explicables (naive/MA/WMA/SES/linear/DOW). Sin LLM. Sin clima/festivos externos.");
        limitations.Add("No interpretar correlación como causalidad.");

        if (history.Count == 0)
        {
            return new DiForecastDto("SALES_DAILY", ForecastEngine.Naive, horizonDays, 0, [], [],
                ForecastAccuracyMetrics.Empty, ForecastAccuracyMetrics.Empty, false, "Baja",
                string.Join(" ", limitations) + " Sin ventas Completed en ventana.", DateTime.UtcNow);
        }

        var holdout = Math.Min(7, Math.Max(1, history.Count / 4));
        var model = history.Count >= holdout + 3 ? ForecastEngine.SelectBestModel(history, holdout) : ForecastEngine.Naive;
        var backtest = ForecastEngine.Backtest(model, history, holdout);
        var pointsRaw = ForecastEngine.Forecast(model, history, horizonDays);
        var mae = backtest.Metrics.Mae ?? history.Average() * 0.15m;
        var points = pointsRaw.Select((v, i) =>
        {
            var (lo, hi) = ForecastEngine.Interval(v, mae);
            return new DiForecastPoint(i + 1, Math.Round(v, 2), Math.Round(lo, 2), Math.Round(hi, 2));
        }).ToList();

        var confidence = ForecastEngine.ConfidenceLabel(history.Count, backtest.Metrics.Mape, backtest.BeatsNaive);
        if (!backtest.BeatsNaive && backtest.Ok)
            limitations.Add("El modelo seleccionado no supera baseline naive en backtest; se reporta con confianza transparente.");

        var dto = new DiForecastDto("SALES_DAILY", model, horizonDays, history.Count, history, points,
            backtest.Metrics, backtest.NaiveBaseline, backtest.BeatsNaive, confidence,
            string.Join(" ", limitations), DateTime.UtcNow);

        if (persistRun)
        {
            _db.DiForecastRuns.Add(new DiForecastRun
            {
                Id = Guid.NewGuid(),
                CompanyId = filter.CompanyId,
                BranchId = filter.BranchId,
                MetricCode = "SALES_DAILY",
                ModelId = model,
                HorizonDays = horizonDays,
                HistoryPoints = history.Count,
                Mae = backtest.Metrics.Mae,
                Mape = backtest.Metrics.Mape,
                Rmse = backtest.Metrics.Rmse,
                BeatsNaive = backtest.BeatsNaive,
                Confidence = confidence,
                ForecastJson = JsonSerializer.Serialize(points),
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }

        return dto;
    }

    public async Task<IReadOnlyList<DiRecommendationDto>> GetRecommendationsAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        var list = new List<DiRecommendationDto>();

        // Inventory coverage top risks
        try
        {
            var coverage = await _analytics.GetReportDataAsync("inventory-coverage", filter, ct);
            if (coverage is IEnumerable<Dictionary<string, object?>> rows)
            {
                foreach (var row in rows.Take(30))
                {
                    var name = Convert.ToString(row.GetValueOrDefault("product_name") ?? row.GetValueOrDefault("ProductName") ?? "SKU") ?? "SKU";
                    var stock = ToDec(row.GetValueOrDefault("stock") ?? row.GetValueOrDefault("Stock"));
                    var daily = ToDec(row.GetValueOrDefault("avg_daily_consumption") ?? row.GetValueOrDefault("daily_consumption") ?? row.GetValueOrDefault("AvgDaily"));
                    var lead = ToDec(row.GetValueOrDefault("lead_time_days") ?? 3m);
                    if (daily <= 0) daily = ToDec(row.GetValueOrDefault("consumption_30d")) / 30m;
                    var rec = InventoryReorderRules.CoverageRisk(name, stock, daily, lead <= 0 ? 3m : lead);
                    if (rec != null) list.Add(rec);
                }
            }
        }
        catch { /* SP shape varies */ }

        // Fallback low stock via inventory health + products (BI SPs may be absent)
        try
        {
            var inv = await _bi.GetInventoryHealthAsync(filter.CompanyId, filter.BranchId, ct);
            if (inv != null && inv.LowStockCount > 0)
            {
                list.Add(RecommendationComposer.Build(
                    "INV.LOW_STOCK", "Inventory",
                    $"{inv.LowStockCount} productos en stock crítico; {inv.ZeroStockCount} en cero.",
                    "Fuente: analytics.sp_inventory_health / InventoryHealth.",
                    "Priorizar OC para SKUs en cero y críticos; revisar cobertura.",
                    "Reducir quiebres y ventas perdidas estimadas.",
                    "Alta", "inventarista", "Alto"));
            }
        }
        catch { /* soft */ }

        try
        {
            var cash = await _bi.GetCashSummaryAsync(filter.CompanyId, filter.BranchId, filter.StartUtc, filter.EndUtc, ct);
            if (cash != null)
            {
                var risk = CashRiskRules.VarianceRisk(Math.Abs(cash.TotalVariance), 5m,
                    $"total_variance={cash.TotalVariance:N2}; abs={cash.AbsVariance:N2}");
                if (risk != null) list.Add(risk);
            }
        }
        catch { /* soft */ }

        try
        {
            var decisions = await _analytics.GetDecisionsAsync(filter, ct);
            foreach (var d in decisions.Take(10))
            {
                list.Add(RecommendationComposer.Build(
                    d.Code, "AnalyticsDecision", d.Problem,
                    $"KPI {d.MetricCode}; impacto {d.ImpactEstimate}; periodo {d.PeriodLabel}",
                    d.SuggestedAction ?? "Revisar en Analytics",
                    "Mejora operativa / financiera según KPI",
                    d.Priority is "High" or "Critical" ? "Alta" : "Media",
                    "manager",
                    d.Priority is "High" or "Critical" ? "Alto" : "Medio",
                    d.CurrentValue));
            }
        }
        catch { /* soft */ }

        // Deduplicate by code+observation prefix
        return list
            .GroupBy(r => r.Code + "|" + (r.Observation.Length > 80 ? r.Observation[..80] : r.Observation))
            .Select(g => g.First())
            .Take(40)
            .ToList();
    }

    public async Task<DiDecisionRecord> AcceptRecommendationAsync(Guid companyId, Guid? branchId, Guid userId, DiRecommendationDto rec, string? comment, CancellationToken ct = default)
    {
        var entity = new DiDecisionRecord
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BranchId = branchId,
            RecommendationCode = rec.Code,
            Category = rec.Category,
            Observation = rec.Observation,
            Evidence = rec.Evidence,
            RecommendedAction = rec.RecommendedAction,
            ExpectedImpact = rec.ExpectedImpact,
            ExpectedImpactValue = rec.ImpactEstimate,
            Status = DiDecisionStatus.Accepted,
            CreatedByUserId = userId,
            Comment = comment,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        _db.DiDecisionRecords.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<DiDecisionRecord?> UpdateDecisionStatusAsync(Guid companyId, Guid decisionId, Guid userId, DiDecisionStatus status, string? comment, decimal? actualImpact, CancellationToken ct = default)
    {
        var entity = await _db.DiDecisionRecords.FirstOrDefaultAsync(d => d.Id == decisionId && d.CompanyId == companyId, ct);
        if (entity == null) return null;
        entity.Status = status;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        if (comment != null) entity.Comment = comment;
        if (actualImpact.HasValue) entity.ActualImpactValue = actualImpact;
        if (status == DiDecisionStatus.Verified) entity.VerifiedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<IReadOnlyList<DiDecisionRecord>> ListDecisionsAsync(Guid companyId, Guid? branchId, CancellationToken ct = default)
    {
        var q = _db.DiDecisionRecords.AsNoTracking().Where(d => d.CompanyId == companyId);
        if (branchId.HasValue) q = q.Where(d => d.BranchId == null || d.BranchId == branchId);
        return await q.OrderByDescending(d => d.CreatedAtUtc).Take(100).ToListAsync(ct);
    }

    public async Task<DiSimulationResult> SimulateSalesDeltaAsync(AnalyticsFilter filter, decimal pctChange, CancellationToken ct = default)
    {
        pctChange = Math.Clamp(pctChange, -50m, 50m);
        var sales = await _db.Orders.AsNoTracking()
            .Where(o => o.CompanyId == filter.CompanyId
                        && (filter.BranchId == null || o.BranchId == filter.BranchId)
                        && o.Status == OrderStatus.Completed
                        && o.ClosedAt >= filter.StartUtc && o.ClosedAt < filter.EndUtc)
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0m, ct);
        var sim = Math.Round(sales * (1 + pctChange / 100m), 2);
        return new DiSimulationResult(
            $"Ventas {(pctChange >= 0 ? "+" : "")}{pctChange:N1}%",
            sales, sim, sim - sales,
            "Simulación what-if en memoria. No modifica órdenes, caja ni inventario.");
    }

    async Task<List<decimal>> LoadDailySalesAsync(AnalyticsFilter filter, CancellationToken ct)
    {
        // Prefer last 60 completed days ending at filter.EndUtc
        var end = filter.EndUtc;
        var start = end.AddDays(-60);
        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.CompanyId == filter.CompanyId
                        && (filter.BranchId == null || o.BranchId == filter.BranchId)
                        && o.Status == OrderStatus.Completed
                        && o.ClosedAt != null
                        && o.ClosedAt >= start && o.ClosedAt < end)
            .GroupBy(o => o.ClosedAt!.Value.Date)
            .Select(g => new { Day = g.Key, Total = g.Sum(x => (decimal?)x.TotalAmount) ?? 0m })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        if (rows.Count == 0) return [];

        // Fill gaps with 0 to keep DOW alignment
        var map = rows.ToDictionary(r => r.Day, r => r.Total);
        var cursor = rows[0].Day.Date;
        var last = rows[^1].Day.Date;
        var series = new List<decimal>();
        while (cursor <= last)
        {
            series.Add(map.TryGetValue(cursor, out var v) ? v : 0m);
            cursor = cursor.AddDays(1);
        }
        return series;
    }

    static decimal ToDec(object? v)
    {
        if (v == null) return 0;
        if (v is decimal d) return d;
        if (v is double db) return (decimal)db;
        if (v is float f) return (decimal)f;
        if (v is int i) return i;
        if (v is long l) return l;
        return decimal.TryParse(Convert.ToString(v), out var p) ? p : 0;
    }
}
