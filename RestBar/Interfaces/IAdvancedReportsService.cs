using RestBar.ViewModels;

namespace RestBar.Interfaces
{
    public interface IAdvancedReportsService
    {

        

        
        // Análisis de Rentabilidad
        Task<ProfitabilityAnalysis> GetProfitabilityAnalysisAsync(ReportFilters filters);
        Task<List<ProductProfitabilityData>> GetProductProfitabilityAsync(ReportFilters filters);
        Task<List<CategoryProfitabilityData>> GetCategoryProfitabilityAsync(ReportFilters filters);
        
        // Análisis de Tendencias
        Task<TrendAnalysisReport> GetTrendAnalysisAsync(ReportFilters filters);
        Task<List<SeasonalTrendData>> GetSeasonalTrendsAsync(ReportFilters filters);
        Task<List<GrowthAnalysisData>> GetGrowthAnalysisAsync(ReportFilters filters);
        
        // Reportes de Auditoría
        Task<AuditReport> GetAuditReportAsync(ReportFilters filters);
        Task<List<UserActivityData>> GetUserActivityReportAsync(ReportFilters filters);
        Task<List<SystemLogData>> GetSystemLogReportAsync(ReportFilters filters);
        

        
        // Reportes de Configuración Avanzada
        Task<SystemHealthReport> GetSystemHealthReportAsync();
        Task<List<PerformanceMetricsData>> GetPerformanceMetricsAsync(ReportFilters filters);
        
        // Exportación
        Task<byte[]> ExportAdvancedReportToPdfAsync(string reportType, ReportFilters filters);
        Task<byte[]> ExportAdvancedReportToExcelAsync(string reportType, ReportFilters filters);
    }
} 