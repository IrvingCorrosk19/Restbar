namespace RestBar.Domain.Intelligence;

public static class BiDecisionMath
{
    public static decimal ClampScore(decimal value) => Math.Clamp(Math.Round(value, 2), 0, 100);

    public static decimal EnterpriseScore(decimal financial, decimal operational, decimal foodCost, decimal procurement) =>
        ClampScore(0.30m * financial + 0.25m * operational + 0.25m * foodCost + 0.20m * procurement);

    public static decimal FinancialScore(decimal revenueToday, decimal revenueYesterday, decimal grossMarginPct)
    {
        var growth = revenueYesterday <= 0 ? 50m :
            ClampScore(50m + (revenueToday - revenueYesterday) / Math.Max(revenueYesterday, 1m) * 100m);
        var margin = ClampScore(grossMarginPct); // already %
        return ClampScore(0.5m * growth + 0.5m * margin);
    }

    public static decimal FoodCostHealthScore(decimal theoPct, decimal actualPct, decimal variancePts)
    {
        // Ideal FC ~28-35; penalize high actual and high variance
        var fcScore = actualPct <= 0 ? 70m : ClampScore(100m - Math.Abs(actualPct - 30m) * 2m);
        var varScore = ClampScore(100m - Math.Abs(variancePts) * 10m);
        return ClampScore(0.6m * fcScore + 0.4m * varScore);
    }

    public static decimal ProcurementHealthScore(int openPos, int overdue, int criticalSuppliers)
    {
        var baseScore = 100m;
        baseScore -= overdue * 15m;
        baseScore -= criticalSuppliers * 10m;
        baseScore -= Math.Max(0, openPos - 10) * 2m;
        return ClampScore(baseScore);
    }

    public static decimal OperationalScore(int ordersToday, decimal avgTicket)
    {
        var orderScore = ClampScore(ordersToday * 5m);
        var ticketScore = ClampScore(avgTicket); // crude
        return ClampScore(0.7m * orderScore + 0.3m * Math.Min(ticketScore, 100m));
    }

    public static decimal SalesDropPercent(decimal today, decimal yesterday)
    {
        if (yesterday <= 0) return 0;
        return Math.Round((yesterday - today) / yesterday * 100m, 2);
    }
}
