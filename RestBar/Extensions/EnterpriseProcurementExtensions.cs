using RestBar.Interfaces;
using RestBar.Services.Procurement;

namespace RestBar.Extensions;

public static class EnterpriseProcurementExtensions
{
    public static IServiceCollection AddEnterpriseProcurementModule(this IServiceCollection services)
    {
        services.AddScoped<IProcurementIntegrityService, ProcurementIntegrityService>();
        services.AddScoped<IProcurementCostEngine, ProcurementCostEngine>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<ISupplierScoreService, SupplierScoreService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
        services.AddScoped<IGoodsReceiptService, GoodsReceiptService>();
        services.AddScoped<IProcurementDashboardService, ProcurementDashboardService>();
        services.AddScoped<IProcurementReportService, ProcurementReportService>();
        return services;
    }
}
