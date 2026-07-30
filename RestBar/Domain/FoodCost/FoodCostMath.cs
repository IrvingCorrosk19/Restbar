using RestBar.Models;

namespace RestBar.Domain.FoodCost;

public static class FoodCostMath
{
    public static decimal EffectiveIngredientQty(decimal recipeQty, decimal wastePercent) =>
        recipeQty * (1m + wastePercent / 100m);

    public static decimal ApplyYield(decimal rawCost, decimal yieldPercent)
    {
        if (yieldPercent <= 0) return rawCost;
        return Math.Round(rawCost * (100m / yieldPercent), 4);
    }

    public static decimal FoodCostPercent(decimal plateCost, decimal sellingPrice)
    {
        if (sellingPrice <= 0) return 0;
        return Math.Round(plateCost / sellingPrice * 100m, 4);
    }

    public static decimal GrossMargin(decimal sellingPrice, decimal plateCost) =>
        Math.Round(sellingPrice - plateCost, 4);

    public static decimal GrossMarginPercent(decimal sellingPrice, decimal plateCost)
    {
        if (sellingPrice <= 0) return 0;
        return Math.Round((sellingPrice - plateCost) / sellingPrice * 100m, 4);
    }

    public static decimal VarianceAmount(decimal actual, decimal theoretical) =>
        Math.Round(actual - theoretical, 2);

    public static decimal VariancePoints(decimal actualPercent, decimal theoreticalPercent) =>
        Math.Round(actualPercent - theoreticalPercent, 4);

    public static decimal PercentOfSales(decimal amount, decimal sales)
    {
        if (sales <= 0) return 0;
        return Math.Round(amount / sales * 100m, 4);
    }
}

public static class MenuEngineeringClassifier
{
    public static MenuQuadrant Classify(decimal popularityIndex, decimal profitabilityIndex, decimal popMedian, decimal profitMedian)
    {
        var hiPop = popularityIndex >= popMedian;
        var hiProfit = profitabilityIndex >= profitMedian;
        if (hiPop && hiProfit) return MenuQuadrant.Star;
        if (hiPop && !hiProfit) return MenuQuadrant.PlowHorse;
        if (!hiPop && hiProfit) return MenuQuadrant.Puzzle;
        return MenuQuadrant.Dog;
    }

    public static string Recommend(MenuQuadrant q) => q switch
    {
        MenuQuadrant.Star => "Promocionar y proteger margen",
        MenuQuadrant.PlowHorse => "Subir precio o reformular receta",
        MenuQuadrant.Puzzle => "Promocionar / upsell / mejorar visibilidad",
        MenuQuadrant.Dog => "Eliminar o rediseñar",
        _ => "Revisar"
    };
}
