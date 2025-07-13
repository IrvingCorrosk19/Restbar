using RestBar.ViewModels;

namespace RestBar.Interfaces
{
    public interface ISalesReportService
    {
        // ✅ Métricas generales de ventas
        Task<SalesMetrics> GetSalesMetricsAsync(ReportFilters filters);
        
        // ✅ Ventas por día/semana/mes/año
        Task<List<DailySalesData>> GetDailySalesAsync(ReportFilters filters);
        
        // ✅ Top productos más vendidos
        Task<List<TopProductData>> GetTopProductsAsync(ReportFilters filters);
        
        // ✅ Ventas por categoría
        Task<List<CategorySalesData>> GetCategorySalesAsync(ReportFilters filters);
        
        // ✅ Ventas por empleado/mesero
        Task<List<EmployeeSalesData>> GetEmployeeSalesAsync(ReportFilters filters);
        
        // ✅ Ventas por sucursal
        Task<List<BranchSalesData>> GetBranchSalesAsync(ReportFilters filters);
        
        // ✅ Reporte de descuentos y promociones
        Task<List<DiscountData>> GetDiscountsAsync(ReportFilters filters);
        
        // ✅ Reporte completo de ventas
        Task<SalesReportViewModel> GetCompleteSalesReportAsync(ReportFilters filters);
    }
} 