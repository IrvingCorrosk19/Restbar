using RestBar.Infrastructure.Copilot;
using RestBar.Interfaces;
using RestBar.Services.Copilot;

namespace RestBar.Extensions;

public static class EnterpriseCopilotExtensions
{
    public static IServiceCollection AddEnterpriseCopilotModule(this IServiceCollection services)
    {
        services.AddScoped<IAiProvider, DeterministicAiProvider>();
        services.AddScoped<ICopilotIntentClassifier, CopilotIntentClassifier>();
        services.AddScoped<ICopilotMemoryService, CopilotMemoryService>();
        services.AddScoped<ICopilotAuditService, CopilotAuditService>();
        services.AddScoped<ICopilotDecisionService, CopilotDecisionService>();
        services.AddScoped<ICopilotActionService, CopilotActionService>();
        services.AddScoped<ICopilotToolRegistry, CopilotToolRegistry>();
        services.AddScoped<ICopilotOrchestrator, CopilotOrchestratorService>();

        services.AddScoped<ICopilotTool, ExecutiveSnapshotTool>();
        services.AddScoped<ICopilotTool, FoodCostTool>();
        services.AddScoped<ICopilotTool, ProcurementTool>();
        services.AddScoped<ICopilotTool, CashStatusTool>();

        return services;
    }
}
