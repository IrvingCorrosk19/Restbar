using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RestBar.Domain.Intelligence;
using RestBar.Infrastructure.Intelligence;
using RestBar.Interfaces;
using RestBar.Models;
using RestBar.ViewModels;

namespace RestBar.Services.Intelligence;

public class BiInsightEngine : IBiInsightEngine
{
    public IReadOnlyList<BiInsightDraft> Generate(ExecutiveSignals s)
    {
        var list = new List<BiInsightDraft>();
        var drop = BiDecisionMath.SalesDropPercent(s.RevenueToday, s.RevenueYesterday);
        if (drop >= 20m)
            list.Add(new(BiInsightType.SalesDrop, BiSeverity.High,
                "Las ventas bajaron vs ayer",
                $"Hoy ${s.RevenueToday:F2} vs ayer ${s.RevenueYesterday:F2} (−{drop:F1}%). Posibles causas: mix débil, tráfico o ticket bajo.",
                "Revisar top productos, promociones y ocupación de mesas ahora."));

        if (s.ActualFoodCostPct >= 35m || Math.Abs(s.VariancePts) >= 2m)
            list.Add(new(BiInsightType.FoodCostHigh, BiSeverity.High,
                "Food Cost / variance fuera de control",
                $"FC actual {s.ActualFoodCostPct:F1}% · teórico {s.TheoFoodCostPct:F1}% · variance {s.VariancePts:F2} pts.",
                "Abrir Food Cost Command Center · revisar waste y recetas Dogs."));

        if (s.WasteToday >= 50m)
            list.Add(new(BiInsightType.WasteSpike, BiSeverity.Medium,
                "Merma elevada hoy",
                $"Waste registrado ${s.WasteToday:F2} impacta el Food Cost actual.",
                "Auditar WasteEvent del día y responsables de estación."));

        if (s.OverdueOrders > 0)
            list.Add(new(BiInsightType.OverduePurchases, BiSeverity.High,
                "Compras atrasadas",
                $"{s.OverdueOrders} PO(s) pasaron la fecha de entrega esperada.",
                "Contactar proveedores · reprogramar recepción · evaluar preferred supplier."));

        if (s.CriticalSuppliers > 0)
            list.Add(new(BiInsightType.SupplierCritical, BiSeverity.Medium,
                "Proveedores en riesgo",
                $"{s.CriticalSuppliers} proveedor(es) con score bajo o blacklist.",
                "Abrir Procurement · renegociar o cambiar supplier recomendado."));

        if (s.LowStockCount > 0)
            list.Add(new(BiInsightType.LowStock, BiSeverity.Medium,
                "Inventario bajo mínimo",
                $"{s.LowStockCount} productos en o bajo MinStock.",
                "Generar PR/PO desde Command Center de Compras."));

        if (s.ActiveCashSessions == 0 && s.OrdersToday > 0)
            list.Add(new(BiInsightType.CashRisk, BiSeverity.Medium,
                "Ventas sin sesión de caja visible",
                "Hay órdenes hoy pero no hay sesiones de caja activas en el snapshot (módulo off o sin apertura).",
                "Verificar FeatureFlags Cash y apertura de caja."));

        if (list.Count == 0)
            list.Add(new(BiInsightType.Opportunity, BiSeverity.Info,
                "Operación estable",
                "No hay alertas críticas en los umbrales actuales. Buen momento para impulsar Stars.",
                "Promocionar productos Star del Menu Engineering."));

        return list;
    }
}

public class BiAlertEngine : IBiAlertEngine
{
    public IReadOnlyList<BiAlertDraft> Evaluate(ExecutiveSignals s)
    {
        var alerts = new List<BiAlertDraft>();
        if (Math.Abs(s.VariancePts) >= 2m)
            alerts.Add(new("FC_VARIANCE", BiSeverity.High, $"Variance Food Cost {s.VariancePts:F2} pts", "FoodCost"));
        if (s.WasteToday >= 50m)
            alerts.Add(new("WASTE_SPIKE", BiSeverity.Medium, $"Waste hoy ${s.WasteToday:F2}", "FoodCost"));
        if (s.OverdueOrders > 0)
            alerts.Add(new("PO_OVERDUE", BiSeverity.High, $"{s.OverdueOrders} PO atrasados", "Procurement"));
        if (s.CriticalSuppliers > 0)
            alerts.Add(new("SUPPLIER_CRITICAL", BiSeverity.Medium, $"{s.CriticalSuppliers} proveedores críticos", "Procurement"));
        if (s.LowStockCount > 0)
            alerts.Add(new("LOW_STOCK", BiSeverity.Medium, $"{s.LowStockCount} SKUs bajo mínimo", "Inventory"));
        var drop = BiDecisionMath.SalesDropPercent(s.RevenueToday, s.RevenueYesterday);
        if (drop >= 20m)
            alerts.Add(new("SALES_DROP", BiSeverity.High, $"Ventas −{drop:F1}% vs ayer", "Sales"));
        return alerts;
    }
}

public class BiScoreEngine : IBiScoreEngine
{
    public BiScoreResult Compute(ExecutiveSignals s)
    {
        var fin = BiDecisionMath.FinancialScore(s.RevenueToday, s.RevenueYesterday, s.GrossMarginPct);
        var ops = BiDecisionMath.OperationalScore(s.OrdersToday, s.AverageTicket);
        var fc = BiDecisionMath.FoodCostHealthScore(s.TheoFoodCostPct, s.ActualFoodCostPct, s.VariancePts);
        var proc = BiDecisionMath.ProcurementHealthScore(s.OpenPurchaseOrders, s.OverdueOrders, s.CriticalSuppliers);
        var enterprise = BiDecisionMath.EnterpriseScore(fin, ops, fc, proc);
        return new BiScoreResult(enterprise, fin, ops, fc, proc);
    }
}

public class ExecutiveCommandCenterService : IExecutiveCommandCenterService
{
    private static readonly ConcurrentDictionary<Guid, (DateTime At, ExecutiveCommandCenterDto Dto)> Cache = new();
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private readonly ISalesReportService _sales;
    private readonly ICashReportService _cash;
    private readonly IProcurementDashboardService _procurement;
    private readonly IFoodCostDashboardService _foodCost;
    private readonly IBiInsightEngine _insights;
    private readonly IBiAlertEngine _alerts;
    private readonly IBiScoreEngine _scores;
    private readonly RestBarContext _db;

    public ExecutiveCommandCenterService(
        ISalesReportService sales,
        ICashReportService cash,
        IProcurementDashboardService procurement,
        IFoodCostDashboardService foodCost,
        IBiInsightEngine insights,
        IBiAlertEngine alerts,
        IBiScoreEngine scores,
        RestBarContext db)
    {
        _sales = sales;
        _cash = cash;
        _procurement = procurement;
        _foodCost = foodCost;
        _insights = insights;
        _alerts = alerts;
        _scores = scores;
        _db = db;
    }

    public async Task<ExecutiveCommandCenterDto> GetSnapshotAsync(Guid companyId, Guid branchId, Guid? actorUserId = null, CancellationToken ct = default)
    {
        if (Cache.TryGetValue(branchId, out var cached) && DateTime.UtcNow - cached.At < CacheTtl)
            return cached.Dto;

        var sw = Stopwatch.StartNew();
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var yesterday = today.AddDays(-1);

        var salesTodayTask = _sales.GetSalesMetricsAsync(new ReportFilters { BranchId = branchId, StartDate = today, EndDate = tomorrow });
        var salesYdayTask = _sales.GetSalesMetricsAsync(new ReportFilters { BranchId = branchId, StartDate = yesterday, EndDate = today });
        var cashTask = Safe(() => _cash.GetDashboardSnapshotAsync(branchId, ct));
        var procTask = Safe(() => _procurement.GetCommandCenterAsync(companyId, branchId, ct));
        var fcTask = Safe(() => _foodCost.GetCommandCenterAsync(companyId, branchId, ct));

        await Task.WhenAll(salesTodayTask, salesYdayTask, cashTask, procTask, fcTask);

        var salesToday = await salesTodayTask;
        var salesYday = await salesYdayTask;
        var cash = await cashTask;
        var proc = await procTask;
        var fc = await fcTask;

        var expectedCash = GetDecimal(cash, "TotalExpectedCash");
        var activeCash = GetInt(cash, "ActiveSessions", GetCount(cash, "ActiveSessions"));
        var openPo = GetInt(proc, "OpenPurchaseOrders");
        var overdue = GetInt(proc, "OverdueOrders");
        var critical = GetCount(proc, "CriticalSuppliers");
        var lowStock = GetCount(proc, "LowStockItems");

        decimal theoFc = 0, actualFc = 0, variance = 0, waste = GetDecimal(fc, "WasteToday"), marginPct = 0;
        var todayFc = GetProp(fc, "Today");
        if (todayFc != null)
        {
            theoFc = GetDecimal(todayFc, "TheoFoodCostPercent");
            actualFc = GetDecimal(todayFc, "ActualFoodCostPercent");
            variance = GetDecimal(todayFc, "VariancePoints");
            marginPct = GetDecimal(todayFc, "GrossMarginPercent");
            if (marginPct == 0 && salesToday.TotalRevenue > 0)
                marginPct = 100m - actualFc;
        }
        else if (salesToday.ProfitMargin > 0)
            marginPct = salesToday.ProfitMargin;

        var signals = new ExecutiveSignals(
            salesToday.TotalRevenue, salesYday.TotalRevenue, salesToday.TotalOrders, salesToday.AverageTicket,
            marginPct, theoFc, actualFc, variance, waste, openPo, overdue, critical, lowStock,
            expectedCash, activeCash);

        var insightList = _insights.Generate(signals);
        var alertList = _alerts.Evaluate(signals);
        var score = _scores.Compute(signals);
        var drop = BiDecisionMath.SalesDropPercent(signals.RevenueToday, signals.RevenueYesterday);

        var topActions = insightList
            .OrderByDescending(i => i.Severity)
            .Select(i => i.Action)
            .Distinct()
            .Take(5)
            .ToList();

        sw.Stop();
        var dto = new ExecutiveCommandCenterDto(
            score.Enterprise, score,
            signals.RevenueToday, signals.RevenueYesterday, drop,
            signals.OrdersToday, signals.AverageTicket,
            signals.ExpectedCash, signals.ActiveCashSessions,
            signals.TheoFoodCostPct, signals.ActualFoodCostPct, signals.VariancePts, signals.WasteToday,
            signals.OpenPurchaseOrders, signals.OverdueOrders, signals.CriticalSuppliers, signals.LowStockCount,
            insightList, alertList, topActions, DateTime.UtcNow, (int)sw.ElapsedMilliseconds);

        // Persist lightweight snapshot + audit (best effort)
        try
        {
            _db.ExecutiveSnapshots.Add(new ExecutiveSnapshot
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                BranchId = branchId,
                PeriodType = "Today",
                SnapshotJson = JsonSerializer.Serialize(new { dto.EnterpriseScore, dto.RevenueToday, dto.ActualFoodCostPct, dto.OpenPurchaseOrders }),
                EnterpriseScore = dto.EnterpriseScore
            });

            foreach (var a in alertList.Take(10))
            {
                _db.BiAlerts.Add(new BiAlert
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    AlertCode = a.Code,
                    Severity = a.Severity,
                    Message = a.Message,
                    SourceModule = a.SourceModule
                });
            }

            _db.BiScores.Add(new BiScore
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                BranchId = branchId,
                SubjectType = BiSubjectType.Branch,
                SubjectId = branchId,
                Score = score.Enterprise,
                DimensionsJson = JsonSerializer.Serialize(score)
            });

            // Forecast seeds (historical only — predictive ready)
            _db.ForecastSeeds.Add(new ForecastSeed
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                BranchId = branchId,
                MetricCode = "Revenue",
                AsOfDate = today,
                Value = signals.RevenueToday,
                Source = "Historical"
            });

            if (actorUserId.HasValue)
            {
                var audit = new BiAuditEvent
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    BranchId = branchId,
                    ActorUserId = actorUserId.Value,
                    QueryName = "ExecutiveCommandCenter.GetSnapshot",
                    FiltersJson = $"{{\"branchId\":\"{branchId}\"}}",
                    DurationMs = dto.DurationMs
                };
                audit.EventHash = BiHashChainBuilder.Compute(audit);
                _db.BiAuditEvents.Add(audit);
            }

            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // Never fail the CC UI on persistence
        }

        Cache[branchId] = (DateTime.UtcNow, dto);
        return dto;
    }

    private static async Task<object?> Safe(Func<Task<object>> factory)
    {
        try { return await factory(); }
        catch { return null; }
    }

    private static object? GetProp(object? obj, string name)
    {
        if (obj == null) return null;
        var p = obj.GetType().GetProperty(name);
        return p?.GetValue(obj);
    }

    private static decimal GetDecimal(object? obj, string name)
    {
        var v = GetProp(obj, name);
        return v switch
        {
            null => 0m,
            decimal d => d,
            double db => (decimal)db,
            float f => (decimal)f,
            int i => i,
            long l => l,
            _ => decimal.TryParse(v.ToString(), out var x) ? x : 0m
        };
    }

    private static int GetInt(object? obj, string name, int fallback = 0)
    {
        var v = GetProp(obj, name);
        return v switch
        {
            null => fallback,
            int i => i,
            long l => (int)l,
            decimal d => (int)d,
            _ => int.TryParse(v.ToString(), out var x) ? x : fallback
        };
    }

    private static int GetCount(object? obj, string name)
    {
        var v = GetProp(obj, name);
        if (v is System.Collections.ICollection c) return c.Count;
        if (v is System.Collections.IEnumerable e && v is not string)
        {
            var n = 0;
            foreach (var _ in e) n++;
            return n;
        }
        return GetInt(obj, name);
    }
}
