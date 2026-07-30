using RestBar.Interfaces;
using RestBar.Services.Intelligence;

namespace RestBar.Extensions;

public static class EnterpriseIntelligenceExtensions
{
    public static IServiceCollection AddEnterpriseIntelligenceModule(this IServiceCollection services)
    {
        services.AddScoped<IBiInsightEngine, BiInsightEngine>();
        services.AddScoped<IBiAlertEngine, BiAlertEngine>();
        services.AddScoped<IBiScoreEngine, BiScoreEngine>();
        services.AddScoped<IExecutiveCommandCenterService, ExecutiveCommandCenterService>();
        return services;
    }
}
