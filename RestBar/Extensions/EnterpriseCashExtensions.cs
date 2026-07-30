using RestBar.Infrastructure.Cash;
using RestBar.Interfaces;
using RestBar.Services.Cash;

namespace RestBar.Extensions;

public static class EnterpriseCashExtensions
{
    public static IServiceCollection AddEnterpriseCashModule(this IServiceCollection services)
    {
        services.AddScoped<ICashRegisterService, CashRegisterService>();
        services.AddScoped<ICashMovementService, CashMovementService>();
        services.AddScoped<ICashIntegrityService, CashIntegrityService>();
        services.AddScoped<ICashApprovalService, CashApprovalService>();
        services.AddScoped<ICashReconciliationService, CashReconciliationService>();
        services.AddScoped<ICashReportService, CashReportService>();
        services.AddScoped<ICashSessionService, CashSessionService>();
        services.AddScoped<ICashPaymentHook, CashPaymentHook>();
        return services;
    }
}
