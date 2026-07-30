using RestBar.Domain.FoodCost;
using RestBar.Models;
using RestBar.Services.FoodCost;

namespace RestBar.Tests.FoodCost;

public class FoodCostMathTests
{
    [Fact]
    public void EffectiveQty_IncludesWaste()
    {
        Assert.Equal(1.1m, FoodCostMath.EffectiveIngredientQty(1m, 10m));
    }

    [Fact]
    public void ApplyYield_ScalesCost()
    {
        Assert.Equal(12.5m, FoodCostMath.ApplyYield(10m, 80m));
    }

    [Fact]
    public void FoodCostPercent_Basic()
    {
        Assert.Equal(30m, FoodCostMath.FoodCostPercent(3m, 10m));
    }

    [Fact]
    public void VariancePoints()
    {
        Assert.Equal(2m, FoodCostMath.VariancePoints(32m, 30m));
    }
}

public class MenuEngineeringClassifierTests
{
    [Theory]
    [InlineData(10, 10, 5, 5, MenuQuadrant.Star)]
    [InlineData(10, 1, 5, 5, MenuQuadrant.PlowHorse)]
    [InlineData(1, 10, 5, 5, MenuQuadrant.Puzzle)]
    [InlineData(1, 1, 5, 5, MenuQuadrant.Dog)]
    public void Classify(decimal pop, decimal profit, decimal popMed, decimal profitMed, MenuQuadrant expected)
    {
        Assert.Equal(expected, MenuEngineeringClassifier.Classify(pop, profit, popMed, profitMed));
    }
}

public class CostSimulationTests
{
    [Fact]
    public void Simulate_PriceIncrease_ImprovesMargin()
    {
        var svc = new CostSimulationService();
        var r = svc.Simulate(new RestBar.Interfaces.CostSimulationRequest(10m, 3m, NewPrice: 12m));
        Assert.True(r.NewMargin > 7m);
        Assert.True(r.NewFoodCostPercent < 30m);
    }
}
