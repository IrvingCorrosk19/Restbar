using Microsoft.AspNetCore.Authorization;
using RestBar.Infrastructure.Foundation;

namespace RestBar.Extensions;

/// <summary>
/// DI registration helpers for enterprise foundation. Keeps Program.cs thinner over time.
/// </summary>
public static class EnterpriseFoundationExtensions
{
    public static IServiceCollection AddEnterpriseFoundation(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FeatureFlags>(configuration.GetSection(FeatureFlags.SectionName));
        services.AddScoped<ITenantScopeAccessor, TenantScopeAccessor>();
        return services;
    }

    public static AuthorizationOptions AddEnterpriseModulePolicies(this AuthorizationOptions options)
    {
        // Future modules — do not alter existing policy behavior.
        options.AddPolicy("CashAccess", policy =>
            policy.RequireRole("admin", "manager", "cashier", "accountant", "supervisor"));

        options.AddPolicy("PurchasingAccess", policy =>
            policy.RequireRole("admin", "manager", "inventarista", "accountant", "supervisor"));

        options.AddPolicy("CostingAccess", policy =>
            policy.RequireRole("admin", "manager", "accountant", "chef"));

        options.AddPolicy("FranchiseAccess", policy =>
            policy.RequireRole("admin", "superadmin"));

        return options;
    }
}
