namespace RestBar.Domain.DecisionIntelligence;

/// <summary>Structured, evidence-backed recommendations. Not accusations.</summary>
public static class RecommendationComposer
{
    public static DiRecommendationDto Build(
        string code,
        string category,
        string observation,
        string evidence,
        string action,
        string expectedImpact,
        string confidence,
        string ownerRole,
        string severity,
        decimal? impactEstimate = null)
        => new(
            Code: code,
            Category: category,
            Observation: observation,
            Evidence: evidence,
            BusinessRule: code,
            RecommendedAction: action,
            ExpectedImpact: expectedImpact,
            Confidence: confidence,
            OwnerRole: ownerRole,
            Severity: severity,
            ImpactEstimate: impactEstimate,
            Status: "NEW");
}

public sealed record DiRecommendationDto(
    string Code,
    string Category,
    string Observation,
    string Evidence,
    string BusinessRule,
    string RecommendedAction,
    string ExpectedImpact,
    string Confidence,
    string OwnerRole,
    string Severity,
    decimal? ImpactEstimate,
    string Status);

/// <summary>Deterministic cash/ops risk signals — never auto-label fraud.</summary>
public static class CashRiskRules
{
    public static DiRecommendationDto? VarianceRisk(decimal varianceAbs, decimal threshold, string sessionInfo)
    {
        if (varianceAbs < threshold) return null;
        var sev = varianceAbs >= threshold * 3 ? "Alto" : varianceAbs >= threshold * 1.5m ? "Medio" : "Bajo";
        return RecommendationComposer.Build(
            "CASH.VARIANCE",
            "CashRisk",
            $"Diferencia de caja observada {varianceAbs:N2}.",
            $"Sesión/agregado: {sessionInfo}. Umbral configurado: {threshold:N2}.",
            "Revisar arqueo, movimientos manuales y permisos del turno. No asumir fraude.",
            "Reducir exposición a faltantes no explicados.",
            sev == "Alto" ? "Alta" : "Media",
            "manager",
            sev,
            -varianceAbs);
    }
}

public static class InventoryReorderRules
{
    public static DiRecommendationDto? CoverageRisk(
        string productName,
        decimal stock,
        decimal avgDailyConsumption,
        decimal leadTimeDays,
        decimal safetyDays = 2m)
    {
        if (avgDailyConsumption <= 0) return null;
        var coverage = stock / avgDailyConsumption;
        var needDays = leadTimeDays + safetyDays;
        if (coverage >= needDays) return null;
        var buyQty = Math.Max(0, avgDailyConsumption * needDays - stock);
        return RecommendationComposer.Build(
            "INV.REORDER",
            "Inventory",
            $"Cobertura de '{productName}' es {coverage:N1} días.",
            $"Stock={stock:N2}; consumo medio/día={avgDailyConsumption:N2}; lead time={leadTimeDays:N1}d; seguridad={safetyDays:N1}d.",
            $"Comprar al menos {buyQty:N2} unidades hoy (o generar OC).",
            "Evitar quiebre de stock en el horizonte lead+seguridad.",
            coverage < leadTimeDays ? "Alta" : "Media",
            "inventarista",
            coverage < leadTimeDays ? "Alto" : "Medio",
            buyQty);
    }
}
