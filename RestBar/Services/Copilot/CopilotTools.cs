using System.Text;
using Microsoft.Extensions.Logging;
using RestBar.Domain.Copilot;
using RestBar.Interfaces;
using RestBar.Models;

namespace RestBar.Services.Copilot;

public class CopilotToolRegistry : ICopilotToolRegistry
{
    private readonly IEnumerable<ICopilotTool> _tools;
    private readonly ILogger<CopilotToolRegistry> _logger;

    public CopilotToolRegistry(IEnumerable<ICopilotTool> tools, ILogger<CopilotToolRegistry> logger)
    {
        _tools = tools;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CopilotToolResult>> InvokeForIntentAsync(
        CopilotIntent intent, CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var selected = _tools.Where(t => t.Intents.Contains(intent) || (intent == CopilotIntent.WhatShouldIDo && t.Name == "get_executive_snapshot")
            || (intent == CopilotIntent.ExecutiveBriefing && t.Name == "get_executive_snapshot")
            || (intent == CopilotIntent.AlertsNow && t.Name == "get_executive_snapshot")
            || (intent == CopilotIntent.RecommendMenu && t.Name == "get_executive_snapshot")
            || (intent == CopilotIntent.WasteStatus && t.Name == "get_food_cost")
            || (intent == CopilotIntent.SupplierAdvice && t.Name == "get_procurement")
            || (intent == CopilotIntent.SalesToday && t.Name == "get_executive_snapshot")
            || (intent == CopilotIntent.FoodCostWhy && t.Name == "get_food_cost")
            || (intent == CopilotIntent.PurchasingWhat && t.Name == "get_procurement")
            || (intent == CopilotIntent.CashStatus && t.Name == "get_cash_status")
            || (intent == CopilotIntent.DraftPurchaseRequest && t.Name == "get_procurement")).ToList();

        // Fallback: executive snapshot for unknown operational questions
        if (selected.Count == 0 && intent is CopilotIntent.Unknown or CopilotIntent.Help)
            return Array.Empty<CopilotToolResult>();

        if (selected.Count == 0)
            selected = _tools.Where(t => t.Name == "get_executive_snapshot").ToList();

        var results = new List<CopilotToolResult>();
        foreach (var tool in selected.DistinctBy(t => t.Name))
        {
            if (!CopilotPolicyMap.HasPolicy(ctx.Role, tool.RequiredPolicy))
            {
                results.Add(new CopilotToolResult(tool.Name, false, $"Acceso denegado a `{tool.Name}` (requiere {tool.RequiredPolicy})."));
                continue;
            }

            try
            {
                results.Add(await tool.InvokeAsync(ctx, ct));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Copilot tool {Tool} failed", tool.Name);
                results.Add(new CopilotToolResult(tool.Name, true, $"No pude completar `{tool.Name}`: {ex.Message}"));
            }
        }

        return results;
    }
}

public class ExecutiveSnapshotTool : ICopilotTool
{
    private readonly IExecutiveCommandCenterService _cc;
    public ExecutiveSnapshotTool(IExecutiveCommandCenterService cc) => _cc = cc;
    public string Name => "get_executive_snapshot";
    public string RequiredPolicy => "ReportAccess";
    public IReadOnlyList<CopilotIntent> Intents { get; } = new[]
    {
        CopilotIntent.ExecutiveBriefing, CopilotIntent.WhatShouldIDo, CopilotIntent.AlertsNow,
        CopilotIntent.SalesToday, CopilotIntent.RecommendMenu
    };

    public async Task<CopilotToolResult> InvokeAsync(CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var snap = await _cc.GetSnapshotAsync(ctx.CompanyId, ctx.BranchId, ctx.UserId, ct);
        var sb = new StringBuilder();
        sb.AppendLine("#### Resumen ejecutivo");
        sb.AppendLine($"- **Score empresarial:** {snap.EnterpriseScore:0.#}/100");
        sb.AppendLine($"- **Ventas hoy:** {snap.RevenueToday:C2} (ayer {snap.RevenueYesterday:C2}, Δ {snap.SalesDropPercent:0.#}%)");
        sb.AppendLine($"- **Órdenes:** {snap.OrdersToday} · Ticket medio {snap.AverageTicket:C2}");
        sb.AppendLine($"- **Food Cost:** teórico {snap.TheoFoodCostPct:0.#}% · real {snap.ActualFoodCostPct:0.#}% · var {snap.VariancePts:0.#} pts");
        sb.AppendLine($"- **Merma hoy:** {snap.WasteToday:C2}");
        sb.AppendLine($"- **Caja esperada:** {snap.ExpectedCash:C2} · sesiones activas {snap.ActiveCashSessions}");
        sb.AppendLine($"- **Compras abiertas:** {snap.OpenPurchaseOrders} · vencidas {snap.OverdueOrders} · proveedores críticos {snap.CriticalSuppliers}");
        sb.AppendLine($"- **Stock bajo:** {snap.LowStockCount}");

        if (snap.Alerts.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("#### Alertas");
            foreach (var a in snap.Alerts.OrderByDescending(x => CopilotDecisionMath.SeverityRank(x.Severity)).Take(6))
                sb.AppendLine($"- [{a.Severity}] {a.Code}: {a.Message} ({a.SourceModule})");
        }

        if (snap.Insights.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("#### Insights");
            foreach (var i in snap.Insights.Take(5))
                sb.AppendLine($"- **{i.Title}** — {i.Explanation} → *{i.Action}*");
        }

        return new CopilotToolResult(Name, true, sb.ToString(), snap);
    }
}

public class FoodCostTool : ICopilotTool
{
    private readonly IExecutiveCommandCenterService _cc;

    public FoodCostTool(IExecutiveCommandCenterService cc) => _cc = cc;

    public string Name => "get_food_cost";
    public string RequiredPolicy => "CostingAccess";
    public IReadOnlyList<CopilotIntent> Intents { get; } = new[] { CopilotIntent.FoodCostWhy, CopilotIntent.WasteStatus };

    public async Task<CopilotToolResult> InvokeAsync(CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var snap = await _cc.GetSnapshotAsync(ctx.CompanyId, ctx.BranchId, ctx.UserId, ct);
        var sb = new StringBuilder();
        sb.AppendLine("#### Análisis Food Cost");
        sb.AppendLine($"El Food Cost real está en **{snap.ActualFoodCostPct:0.#}%** (teórico {snap.TheoFoodCostPct:0.#}%).");
        sb.AppendLine($"Varianza: **{snap.VariancePts:0.#} puntos**.");
        sb.AppendLine();
        sb.AppendLine("Drivers observados (Command Center):");
        if (snap.VariancePts > 2)
            sb.AppendLine($"- Varianza elevada → impacto estimado ~{CopilotDecisionMath.EstimateImpact(BiSeverity.High, snap.RevenueToday):C0}");
        if (snap.WasteToday > 0)
            sb.AppendLine($"- Merma del día: {snap.WasteToday:C2}");
        if (snap.CriticalSuppliers > 0)
            sb.AppendLine($"- {snap.CriticalSuppliers} proveedor(es) crítico(s) pueden estar empujando costo.");
        if (snap.LowStockCount > 0)
            sb.AppendLine($"- {snap.LowStockCount} ítems en stock bajo (sustituciones caras posibles).");

        var fcInsight = snap.Insights.FirstOrDefault(i => i.Type is BiInsightType.FoodCostHigh or BiInsightType.WasteSpike or BiInsightType.NegativeMargin);
        if (fcInsight != null)
            sb.AppendLine($"- Insight: {fcInsight.Explanation}");

        sb.AppendLine();
        sb.AppendLine("**Recomendación:** revisar recetas con merma, renegociar proveedores críticos y validar recepciones vs PO.");
        return new CopilotToolResult(Name, true, sb.ToString(), snap);
    }
}

public class ProcurementTool : ICopilotTool
{
    private readonly IExecutiveCommandCenterService _cc;
    private readonly IProcurementDashboardService _dash;

    public ProcurementTool(IExecutiveCommandCenterService cc, IProcurementDashboardService dash)
    {
        _cc = cc;
        _dash = dash;
    }

    public string Name => "get_procurement";
    public string RequiredPolicy => "PurchasingAccess";
    public IReadOnlyList<CopilotIntent> Intents { get; } = new[]
    {
        CopilotIntent.PurchasingWhat, CopilotIntent.SupplierAdvice, CopilotIntent.DraftPurchaseRequest
    };

    public async Task<CopilotToolResult> InvokeAsync(CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var snap = await _cc.GetSnapshotAsync(ctx.CompanyId, ctx.BranchId, ctx.UserId, ct);
        _ = await _dash.GetCommandCenterAsync(ctx.CompanyId, ctx.BranchId, ct);

        var sb = new StringBuilder();
        sb.AppendLine("#### Compras y proveedores");
        sb.AppendLine($"- PO abiertas: **{snap.OpenPurchaseOrders}**");
        sb.AppendLine($"- PO vencidas: **{snap.OverdueOrders}**");
        sb.AppendLine($"- Proveedores críticos: **{snap.CriticalSuppliers}**");
        sb.AppendLine($"- Stock bajo: **{snap.LowStockCount}**");
        sb.AppendLine();
        if (snap.OverdueOrders > 0)
            sb.AppendLine($"Prioridad: cerrar/seguir {snap.OverdueOrders} PO vencida(s) (impacto ~{CopilotDecisionMath.EstimateImpact(BiSeverity.High, snap.RevenueToday):C0}).");
        if (snap.CriticalSuppliers > 0)
            sb.AppendLine("Recomendación: cambiar o renegociar proveedores con score crítico.");
        if (snap.LowStockCount > 0)
            sb.AppendLine("Recomendación: generar solicitudes de compra para ítems en stock bajo hoy.");
        if (snap.OverdueOrders == 0 && snap.CriticalSuppliers == 0 && snap.LowStockCount == 0)
            sb.AppendLine("Compras estables. Mantén seguimiento de lead times.");

        return new CopilotToolResult(Name, true, sb.ToString(), snap);
    }
}

public class CashStatusTool : ICopilotTool
{
    private readonly IExecutiveCommandCenterService _cc;
    private readonly ICashReportService _cash;

    public CashStatusTool(IExecutiveCommandCenterService cc, ICashReportService cash)
    {
        _cc = cc;
        _cash = cash;
    }

    public string Name => "get_cash_status";
    public string RequiredPolicy => "CashAccess";
    public IReadOnlyList<CopilotIntent> Intents { get; } = new[] { CopilotIntent.CashStatus };

    public async Task<CopilotToolResult> InvokeAsync(CopilotRuntimeContext ctx, CancellationToken ct = default)
    {
        var snap = await _cc.GetSnapshotAsync(ctx.CompanyId, ctx.BranchId, ctx.UserId, ct);
        object? dash = null;
        try { dash = await _cash.GetDashboardSnapshotAsync(ctx.BranchId, ct); } catch { /* soft */ }

        var sb = new StringBuilder();
        sb.AppendLine("#### Estado de caja");
        sb.AppendLine($"- Sesiones activas: **{snap.ActiveCashSessions}**");
        sb.AppendLine($"- Efectivo esperado (señal): **{snap.ExpectedCash:C2}**");
        if (snap.ActiveCashSessions == 0)
            sb.AppendLine("- Atención: no hay sesión abierta — abre caja antes de cobrar en efectivo.");
        else
            sb.AppendLine("- Recomendación: monitorea arqueos y cierra con Z al fin de turno.");
        if (dash != null)
            sb.AppendLine("- Snapshot de caja disponible en módulo Cash.");

        return new CopilotToolResult(Name, true, sb.ToString(), snap);
    }
}
