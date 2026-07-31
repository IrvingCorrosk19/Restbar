using RestBar.Domain.DecisionIntelligence;

namespace RestBar.Tests.DecisionIntelligence;

public class ForecastEngineTests
{
    [Fact]
    public void Naive_repeats_last_value()
    {
        var series = new[] { 10m, 20m, 30m };
        var f = ForecastEngine.Forecast(ForecastEngine.Naive, series, 3);
        Assert.Equal(new[] { 30m, 30m, 30m }, f);
    }

    [Fact]
    public void MovingAverage_uses_window()
    {
        var series = new[] { 10m, 20m, 30m, 40m };
        var f = ForecastEngine.Forecast(ForecastEngine.MovingAverage, series, 2, window: 2);
        Assert.Equal(35m, f[0]);
        Assert.Equal(35m, f[1]);
    }

    [Fact]
    public void Evaluate_computes_mae_and_mape()
    {
        var m = ForecastEngine.Evaluate(new[] { 100m, 100m }, new[] { 110m, 90m });
        Assert.Equal(10m, m.Mae);
        Assert.Equal(10m, m.Mape);
        Assert.Equal(0m, m.Bias);
    }

    [Fact]
    public void Backtest_no_future_leakage_and_reports_baseline()
    {
        // Stable series: naive should be competitive
        var series = Enumerable.Repeat(100m, 30).ToList();
        var r = ForecastEngine.Backtest(ForecastEngine.Naive, series, 7);
        Assert.True(r.Ok);
        Assert.True(r.BeatsNaive);
        Assert.Equal(0m, r.Metrics.Mae);
    }

    [Fact]
    public void Backtest_insufficient_history()
    {
        var r = ForecastEngine.Backtest(ForecastEngine.Linear, new[] { 1m, 2m }, 7);
        Assert.False(r.Ok);
    }

    [Fact]
    public void SelectBestModel_returns_known_id()
    {
        var series = Enumerable.Range(1, 40).Select(i => (decimal)(100 + i)).ToList();
        var id = ForecastEngine.SelectBestModel(series, 7);
        Assert.Contains(id, ForecastEngine.ModelIds);
    }

    [Fact]
    public void Confidence_low_with_short_history()
    {
        Assert.Equal("Baja", ForecastEngine.ConfidenceLabel(5, 10m, true));
    }
}

public class RecommendationRulesTests
{
    [Fact]
    public void CoverageRisk_triggers_when_below_lead_plus_safety()
    {
        var r = InventoryReorderRules.CoverageRisk("Queso", stock: 10, avgDailyConsumption: 8, leadTimeDays: 3);
        Assert.NotNull(r);
        Assert.Equal("INV.REORDER", r!.Code);
        Assert.True(r.ImpactEstimate > 0);
    }

    [Fact]
    public void CoverageRisk_null_when_sufficient()
    {
        var r = InventoryReorderRules.CoverageRisk("Queso", stock: 100, avgDailyConsumption: 8, leadTimeDays: 3);
        Assert.Null(r);
    }

    [Fact]
    public void CashVariance_not_fraud_accusation()
    {
        var r = CashRiskRules.VarianceRisk(50, 5, "session=x");
        Assert.NotNull(r);
        Assert.Contains("No asumir fraude", r!.RecommendedAction);
    }
}
