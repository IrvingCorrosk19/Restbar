using RestBar.Interfaces;
using RestBar.Services.FoodCost;

namespace RestBar.Extensions;

public static class EnterpriseFoodCostExtensions
{
    public static IServiceCollection AddEnterpriseFoodCostModule(this IServiceCollection services)
    {
        services.AddScoped<IFoodCostIntegrityService, FoodCostIntegrityService>();
        services.AddScoped<IFoodCostEngine, FoodCostEngine>();
        services.AddScoped<IRecipeProfitabilityService, RecipeProfitabilityService>();
        services.AddScoped<IWasteService, WasteService>();
        services.AddScoped<IMenuEngineeringService, MenuEngineeringService>();
        services.AddScoped<ICostSimulationService, CostSimulationService>();
        services.AddScoped<IFoodCostDashboardService, FoodCostDashboardService>();
        services.AddScoped<IOrderItemCostHook, OrderItemCostHook>();
        return services;
    }
}
