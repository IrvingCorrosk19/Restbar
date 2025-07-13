using RestBar.ViewModels;

namespace RestBar.Interfaces
{
    public interface IAdvancedReportsService
    {
        // Análisis de Rentabilidad
        Task<ProfitabilityAnalysis> GetProfitabilityAnalysisAsync(ReportFilters filters);
        Task<List<ProductProfitability>> GetProductProfitabilityAsync(ReportFilters filters);
        Task<List<CategoryProfitability>> GetCategoryProfitabilityAsync(ReportFilters filters);
        
        // Análisis de Ventas
        Task<SalesAnalysisReport> GetSalesAnalysisAsync(ReportFilters filters);
        Task<List<TopSellingProduct>> GetTopSellingProductsAsync(ReportFilters filters);
        Task<List<TopSellingCategory>> GetTopSellingCategoriesAsync(ReportFilters filters);
        
        // Análisis de Clientes
        Task<CustomerAnalysisReport> GetCustomerAnalysisAsync(ReportFilters filters);
        Task<List<TopCustomer>> GetTopCustomersAsync(ReportFilters filters);
        Task<List<CustomerSegment>> GetCustomerSegmentsAsync(ReportFilters filters);
        
        // Análisis de Operaciones
        Task<OperationalAnalysisReport> GetOperationalAnalysisAsync(ReportFilters filters);
        Task<List<StationPerformance>> GetStationPerformanceAsync(ReportFilters filters);
        Task<List<TableUtilization>> GetTableUtilizationAsync(ReportFilters filters);
        
        // Exportación
        Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters);
        Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters);
    }
} 