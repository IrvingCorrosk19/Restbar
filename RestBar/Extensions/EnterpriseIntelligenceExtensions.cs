using RestBar.Interfaces;
using RestBar.Services.Analytics;
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
        services.AddScoped<IBiNativeAnalyticsService, BiNativeAnalyticsService>();
        services.AddScoped<IAnalyticsScopeService, AnalyticsScopeService>();
        services.AddScoped<IAnalyticsQueryService, AnalyticsQueryService>();
        services.AddScoped<IAnalyticsExportService, AnalyticsExportService>();
        return services;
    }
}
