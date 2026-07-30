using RestBar.Domain.Intelligence;
using RestBar.Services.Intelligence;
using RestBar.Interfaces;

namespace RestBar.Tests.Intelligence;

public class BiDecisionMathTests
{
    [Fact]
    public void EnterpriseScore_Weights()
    {
        var s = BiDecisionMath.EnterpriseScore(100, 100, 100, 100);
        Assert.Equal(100m, s);
    }

    [Fact]
    public void SalesDropPercent()
    {
        Assert.Equal(25m, BiDecisionMath.SalesDropPercent(75m, 100m));
    }

    [Fact]
    public void FoodCostHealth_PenalizesVariance()
    {
        var healthy = BiDecisionMath.FoodCostHealthScore(30, 30, 0);
        var bad = BiDecisionMath.FoodCostHealthScore(30, 40, 10);
        Assert.True(healthy > bad);
    }
}

public class BiInsightEngineTests
{
    [Fact]
    public void Generates_SalesDrop_Insight()
    {
        var engine = new BiInsightEngine();
        var signals = new ExecutiveSignals(80, 100, 10, 8, 60, 28, 30, 2, 0, 1, 0, 0, 0, 100, 1);
        var insights = engine.Generate(signals);
        Assert.Contains(insights, i => i.Type == RestBar.Models.BiInsightType.SalesDrop);
        Assert.All(insights, i => Assert.False(string.IsNullOrWhiteSpace(i.Action)));
    }

    [Fact]
    public void AlertEngine_Flags_Variance()
    {
        var engine = new BiAlertEngine();
        var signals = new ExecutiveSignals(100, 100, 5, 20, 70, 28, 35, 3, 60, 2, 1, 1, 2, 50, 1);
        var alerts = engine.Evaluate(signals);
        Assert.Contains(alerts, a => a.Code == "FC_VARIANCE");
        Assert.Contains(alerts, a => a.Code == "WASTE_SPIKE");
    }
}
